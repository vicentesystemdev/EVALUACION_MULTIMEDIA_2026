#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace CampusGlitch.Editor {
[InitializeOnLoad]
public static class CGAndroidLocalExport {
    const string State="CampusGlitch.AndroidExport",Auto="CampusGlitch.AndroidAutoCommit";
    static readonly string Root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
    static Task<string> fingerprint;static string sourceStatus="Comprobando cambios…";static double nextCheck,nextAuto;static bool invalid=true;
    static CGAndroidLocalExport(){nextAuto=EditorApplication.timeSinceStartup+90;CGMobileLocal.RequestBuild=Queue;CGMobileLocal.DeviceTools=()=>Path.Combine(EditorApplication.applicationContentsPath,"PlaybackEngines","AndroidPlayer","SDK","platform-tools",Application.platform==RuntimePlatform.WindowsEditor?"adb.exe":"adb");CGMobileLocal.SourceStatus=Status;EditorApplication.update+=Update;EditorApplication.projectChanged+=()=>{invalid=true;};}
    public static string Fingerprint(string root){
        using(var hash=SHA256.Create()){var files=new[]{"Assets","Packages","ProjectSettings"}.SelectMany(dir=>Directory.GetFiles(Path.Combine(root,dir),"*",SearchOption.AllDirectories)).Where(p=>!p.Contains(Path.DirectorySeparatorChar+"_Recovery"+Path.DirectorySeparatorChar)&&!p.EndsWith(".tmp",StringComparison.OrdinalIgnoreCase)&&!p.EndsWith(".csproj",StringComparison.OrdinalIgnoreCase)).OrderBy(p=>p,StringComparer.Ordinal).ToArray();
            foreach(string file in files){byte[] name=Encoding.UTF8.GetBytes(file.Substring(root.Length).Replace('\\','/'));hash.TransformBlock(name,0,name.Length,null,0);byte[] bytes;
                if(file.EndsWith(".meta")||file.EndsWith(".asset")||file.EndsWith(".unity"))bytes=Encoding.UTF8.GetBytes(string.Join("\n",File.ReadAllLines(file).Select(s=>s.TrimEnd())));else bytes=File.ReadAllBytes(file);
                using(var part=SHA256.Create()){var digest=part.ComputeHash(bytes);hash.TransformBlock(digest,0,digest.Length,null,0);}
            }hash.TransformFinalBlock(new byte[0],0,0);return BitConverter.ToString(hash.Hash).Replace("-","").ToLowerInvariant();
        }
    }
    public static string Revision(){try{using(var p=new System.Diagnostics.Process()){p.StartInfo=new System.Diagnostics.ProcessStartInfo("git","rev-parse HEAD"){WorkingDirectory=Root,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};p.Start();string value=p.StandardOutput.ReadToEnd();if(!p.WaitForExit(3000)){p.Kill();return "";}return p.ExitCode==0?value.Trim():"";}}catch{return "";}}
    static string Status(){
        if(!string.IsNullOrEmpty(SessionState.GetString(State,"")))return "GENERANDO APK · ESPERA A QUE UNITY TERMINE";
        if(invalid&&fingerprint==null){invalid=false;fingerprint=Task.Run(()=>Fingerprint(Root));sourceStatus="Comprobando cambios del proyecto…";}
        if(fingerprint!=null&&fingerprint.IsCompleted){var package=CGMobileLocal.Load();sourceStatus=fingerprint.IsFaulted?"No se pudo comparar. Vuelve a generar la APK.":!CGMobileLocal.Exists(package)?"AÚN NO HAY APK EN ESTA COMPUTADORA":package.sourceHash==fingerprint.Result?"APK AL DÍA CON ESTE PROYECTO":"HAY CAMBIOS · GENERA UNA NUEVA VERSIÓN";fingerprint=null;}
        return sourceStatus;
    }
    [MenuItem("Campus Glitch/Android/Actualizar APK tras cada commit",false,102)]static void ToggleAuto(){EditorPrefs.SetBool(Auto,!EditorPrefs.GetBool(Auto,true));}
    [MenuItem("Campus Glitch/Android/Actualizar APK tras cada commit",true)]static bool ValidateAuto(){Menu.SetChecked("Campus Glitch/Android/Actualizar APK tras cada commit",EditorPrefs.GetBool(Auto,true));return true;}
    public static void Queue(){
        if(!string.IsNullOrEmpty(SessionState.GetString(State,"")))return;
        if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Guarda la escena antes de generar la APK.");
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))throw new InvalidOperationException("Instala el soporte Android de esta versión de Unity.");
        SessionState.SetInt(State+".Target",(int)EditorUserBuildSettings.activeBuildTarget);SessionState.SetString(State,"prepare");
        if(EditorApplication.isPlaying)EditorApplication.isPlaying=false;
    }
    static void Update(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode||BuildPipeline.isBuildingPlayer)return;
        string state=SessionState.GetString(State,"");if(state=="prepare"){
            try{CGAndroidBuild.Configure();var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CampusGlitch/Resources/CG/Presentation/CampusGlitchIcon.png");if(icon==null)throw new InvalidOperationException("Falta el icono de Campus Glitch.");var sizes=PlayerSettings.GetIconSizes(NamedBuildTarget.Android,IconKind.Any);PlayerSettings.SetIcons(NamedBuildTarget.Android,sizes.Select(_=>icon).ToArray(),IconKind.Any);AssetDatabase.SaveAssets();
                SessionState.SetString(State,"build");if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android&&!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android))throw new InvalidOperationException("No se pudo cambiar a Android.");
            }catch(Exception error){FinishError(error);}return;
        }
        if(state=="build"){Build();return;}
        if(state=="restore"){SessionState.EraseString(State);var target=(BuildTarget)SessionState.GetInt(State+".Target",(int)BuildTarget.StandaloneWindows64);if(EditorUserBuildSettings.activeBuildTarget!=target)EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target),target);invalid=true;return;}
        if(EditorApplication.timeSinceStartup>nextCheck){nextCheck=EditorApplication.timeSinceStartup+2;Status();}
        if(EditorApplication.timeSinceStartup>nextAuto){nextAuto=EditorApplication.timeSinceStartup+30;var package=CGMobileLocal.Load();string revision=Revision();if(EditorPrefs.GetBool(Auto,true)&&CGMobileLocal.Exists(package)&&!string.IsNullOrEmpty(revision)&&package.sourceCommit!=revision&&!SceneManager.GetActiveScene().isDirty&&sourceStatus=="HAY CAMBIOS · GENERA UNA NUEVA VERSIÓN")Queue();}
    }
    static void Build(){
        if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android){FinishError(new Exception("Android no está activo."));return;}
        SessionState.SetString(State,"building");string name=PlayerSettings.productName;int code=PlayerSettings.Android.bundleVersionCode;
        try{
            var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CampusGlitch/Resources/CG/Presentation/CampusGlitchIcon.png");var iconSizes=PlayerSettings.GetIconSizes(NamedBuildTarget.Android,IconKind.Any);PlayerSettings.SetIcons(NamedBuildTarget.Android,iconSizes.Length>0?iconSizes.Select(_=>icon).ToArray():new[]{icon},IconKind.Any);
            var previous=CGMobileLocal.Load();int version=Math.Max(code,previous!=null?previous.versionCode:0)+1;PlayerSettings.productName="Campus Glitch";PlayerSettings.Android.bundleVersionCode=version;AssetDatabase.SaveAssets();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();var report=CGAndroidBuild.BuildCurrent();if(report.summary.result!=BuildResult.Succeeded)throw new Exception("No se pudo generar la APK. Revisa Builds/Android/Reporte_Build_Android.txt.");
            string ready=Path.Combine(Root,"Builds","Android","CampusGlitch.apk");if(File.Exists(ready))File.Replace(report.summary.outputPath,ready,null);else File.Move(report.summary.outputPath,ready);
            PlayerSettings.productName=name;PlayerSettings.Android.bundleVersionCode=code;AssetDatabase.SaveAssets();
            var package=new CGMobilePackage{apkPath=Path.Combine(Root,"Builds","Android","CampusGlitch.apk"),adbPath=Path.Combine(EditorApplication.applicationContentsPath,"PlaybackEngines","AndroidPlayer","SDK","platform-tools",Application.platform==RuntimePlatform.WindowsEditor?"adb.exe":"adb"),versionName=PlayerSettings.bundleVersion,versionCode=version,createdUtc=DateTime.UtcNow.ToString("O"),sourceHash=Fingerprint(Root),sourceCommit=Revision()};package.sha256=CGMobileLocal.Hash(package.apkPath);
            string json=JsonUtility.ToJson(package,true);File.WriteAllText(Path.Combine(Root,"Builds","Android","version.json"),json);Directory.CreateDirectory(Path.GetDirectoryName(CGMobileLocal.IndexPath));File.WriteAllText(CGMobileLocal.IndexPath,json);File.WriteAllText(Path.Combine(Root,"Builds","Android","Estado.txt"),"APK generada correctamente. Versión "+version+"\nSHA-256 "+package.sha256);sourceStatus="APK AL DÍA CON ESTE PROYECTO";
        }catch(Exception error){FinishError(error);}finally{PlayerSettings.productName=name;PlayerSettings.Android.bundleVersionCode=code;AssetDatabase.SaveAssets();SessionState.SetString(State,"restore");}
    }
    static void FinishError(Exception error){Directory.CreateDirectory(Path.Combine(Root,"Builds","Android"));File.WriteAllText(Path.Combine(Root,"Builds","Android","Estado.txt"),"No se generó una versión nueva.\n"+error);SessionState.SetString(State,"restore");UnityEngine.Debug.LogError(error.Message);}
}
}
#endif
