#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CampusGlitch.Editor {
// Android-only export. Windows settings and CGBuildScenes.BuildWindows are left untouched.
public static class CGAndroidBuild {
  public const string AppId="com.unifranz.campusglitch";
  public const string AndroidName="Campus Glitch";
  const string ApkPath="Builds/Android/CampusGlitch.apk";
  const string ReportPath="Docs/Evaluacion_Dia2/Android/Reporte_Build_Android.txt";

  [MenuItem("Campus Glitch/Android/1. Configurar Android (sin compilar)")]
  public static void Configure(){
    var android=NamedBuildTarget.Android;
    PlayerSettings.SetApplicationIdentifier(android,AppId);
    PlayerSettings.SetScriptingBackend(android,ScriptingImplementation.IL2CPP);
    PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
    EditorUserBuildSettings.buildAppBundle=false; // APK, not AAB
    // Landscape only (left or right, rotates with the phone). These settings only affect mobile targets.
    PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
    PlayerSettings.allowedAutorotateToPortrait=false;
    PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
    PlayerSettings.allowedAutorotateToLandscapeLeft=true;
    PlayerSettings.allowedAutorotateToLandscapeRight=true;
    AssetDatabase.SaveAssets();
    Debug.Log("[Campus Glitch Android] Configurado: "+AppId+" | IL2CPP | ARM64 | APK | Landscape");
  }

  [MenuItem("Campus Glitch/Android/2. Generar APK (Builds/Android)")]
  public static void BuildApk(){
    if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
      throw new InvalidOperationException("Falta el módulo Android Build Support (con OpenJDK y Android SDK & NDK Tools). Instálalo desde Unity Hub > Installs > 6000.3.16f1 > Add modules.");
    var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
    foreach(string s in scenes)if(!File.Exists(s))throw new FileNotFoundException("Escena incluida en Build Profiles pero inexistente: "+s);
    if(scenes.Length!=18)throw new InvalidOperationException("Se esperaban 18 escenas activas en Build Profiles; hay "+scenes.Length+".");
    if(scenes[0]!="Assets/CampusGlitch/Scenes/SC_Bootstrap.unity")throw new InvalidOperationException("La primera escena debe ser SC_Bootstrap; es "+scenes[0]);
    Configure();
    if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)
      EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android);
    // productName is shared with Windows: use "Campus Glitch" for the APK only and restore it afterwards.
    string previousName=PlayerSettings.productName;
    Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
    BuildReport report;
    try {
      PlayerSettings.productName=AndroidName;
      report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=ApkPath,target=BuildTarget.Android,options=BuildOptions.None});
    } finally {
      PlayerSettings.productName=previousName;
      AssetDatabase.SaveAssets();
    }
    WriteReport(report,scenes);
    var summary=report.summary;
    Debug.Log("[Campus Glitch Android] "+summary.result+" | errores "+summary.totalErrors+" | "+ApkPath);
    if(summary.result!=BuildResult.Succeeded)throw new Exception("Build Android falló ("+summary.result+"). Revisa la Consola y "+ReportPath);
    if(!Application.isBatchMode)EditorUtility.RevealInFinder(ApkPath);
  }

  static void WriteReport(BuildReport report,string[] scenes){
    var s=report.summary;var sb=new StringBuilder();
    sb.AppendLine("CAMPUS GLITCH - Reporte de build Android (generado por Unity, CGAndroidBuild.BuildApk)");
    sb.AppendLine("Fecha: "+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    sb.AppendLine("Unity: "+Application.unityVersion);
    sb.AppendLine("Resultado: "+s.result);
    sb.AppendLine("Errores: "+s.totalErrors+" | Advertencias: "+s.totalWarnings);
    sb.AppendLine("Duración: "+s.totalTime);
    sb.AppendLine("Salida: "+s.outputPath);
    if(File.Exists(ApkPath))sb.AppendLine("Tamaño APK: "+(new FileInfo(ApkPath).Length/1048576f).ToString("0.0")+" MB");
    sb.AppendLine("Identificador: "+PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android));
    sb.AppendLine("Nombre: "+AndroidName+" | Versión: "+PlayerSettings.bundleVersion+" ("+PlayerSettings.Android.bundleVersionCode+")");
    sb.AppendLine("Backend: "+PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)+" | Arquitecturas: "+PlayerSettings.Android.targetArchitectures);
    sb.AppendLine("Min SDK: "+PlayerSettings.Android.minSdkVersion+" | Target SDK: "+PlayerSettings.Android.targetSdkVersion);
    sb.AppendLine("Orientación: "+PlayerSettings.defaultInterfaceOrientation+" (landscape izq/der)");
    sb.AppendLine("Escenas ("+scenes.Length+"):");
    for(int i=0;i<scenes.Length;i++)sb.AppendLine("  "+i+". "+scenes[i]);
    Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
    File.WriteAllText(ReportPath,sb.ToString());
  }
}
}
#endif
