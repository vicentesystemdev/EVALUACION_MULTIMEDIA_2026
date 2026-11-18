using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace CampusGlitch {
[Serializable] public sealed class CGMobilePackage {
    public string apkPath,adbPath,versionName,createdUtc,sha256,sourceHash,sourceCommit;
    public int versionCode;
}
public sealed class CGMobilePhone {
    public string serial,state,name;
    public bool Ready {get{return state=="device";}}
    public string Caption {get{return name+" · "+(Ready?"LISTO":state=="unauthorized"?"AUTORIZA EN EL TELÉFONO":"SIN CONEXIÓN");}}
}
// Local distribution only. No accounts, upload, remote downloads or background installation.
public static class CGMobileLocal {
    public static string IndexPath {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CampusGlitch","Android","ultima-version.json");}}
    public static Action RequestBuild;
    public static Func<string> DeviceTools;
    public static Func<string> SourceStatus;
    public static CGMobilePackage Load(){try{return JsonUtility.FromJson<CGMobilePackage>(File.ReadAllText(IndexPath));}catch{return new CGMobilePackage{adbPath=DeviceTools!=null?DeviceTools():null};}}
    public static bool Exists(CGMobilePackage p){return p!=null&&!string.IsNullOrEmpty(p.apkPath)&&Path.GetExtension(p.apkPath).Equals(".apk",StringComparison.OrdinalIgnoreCase)&&File.Exists(p.apkPath)&&!string.IsNullOrEmpty(p.sha256);}
    public static string Hash(string path){using(var sha=SHA256.Create())using(var stream=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();}
    public static void Verify(CGMobilePackage p){if(!Exists(p))throw new IOException("Genera primero la APK en esta computadora.");if(!string.Equals(Hash(p.apkPath),p.sha256,StringComparison.OrdinalIgnoreCase))throw new IOException("La APK cambió o está incompleta. Vuelve a generarla.");}
    public static async Task<string> Copy(CGMobilePackage p,string destination){return await Task.Run(()=>{
        Verify(p);if(string.IsNullOrEmpty(destination))return "Copia cancelada.";
        destination=Path.GetFullPath(destination);if(!Path.GetExtension(destination).Equals(".apk",StringComparison.OrdinalIgnoreCase))throw new IOException("El archivo debe terminar en .apk.");
        if(string.Equals(destination,Path.GetFullPath(p.apkPath),StringComparison.OrdinalIgnoreCase))return "La APK ya está en esa ubicación.";
        string temp=destination+".part-"+Guid.NewGuid().ToString("N");try{File.Copy(p.apkPath,temp);if(Hash(temp)!=p.sha256)throw new IOException("La copia no coincide. Inténtalo de nuevo.");if(File.Exists(destination))File.Replace(temp,destination,null);else File.Move(temp,destination);}finally{if(File.Exists(temp))File.Delete(temp);}
        return "APK guardada en la ubicación elegida.";
    });}
    static string Quote(string value){if(value==null||value.IndexOfAny(new[]{'"','\r','\n'})>=0)throw new ArgumentException("Ruta no válida.");return "\""+value+"\"";}
    public static List<CGMobilePhone> ParsePhones(string text){var result=new List<CGMobilePhone>();foreach(string line in text.Split('\n')){
        var parts=line.Trim().Split((char[])null,StringSplitOptions.RemoveEmptyEntries);if(parts.Length<2||!Regex.IsMatch(parts[0],@"^[A-Za-z0-9._:\-]+$")||!(parts[1]=="device"||parts[1]=="unauthorized"||parts[1]=="offline"))continue;
        string model="Teléfono Android";foreach(string part in parts)if(part.StartsWith("model:"))model=part.Substring(6).Replace('_',' ');
        result.Add(new CGMobilePhone{serial=parts[0],state=parts[1],name=model});
    }return result;}
    static async Task<string> Adb(CGMobilePackage p,string args,int timeout){
        if(p==null||string.IsNullOrEmpty(p.adbPath)||!File.Exists(p.adbPath)||!Path.GetFileName(p.adbPath).Equals(Application.platform==RuntimePlatform.WindowsEditor||Application.platform==RuntimePlatform.WindowsPlayer?"adb.exe":"adb",StringComparison.OrdinalIgnoreCase))throw new IOException("Las herramientas USB estarán disponibles al generar la APK desde Unity.");
        using(var process=new Process()){process.StartInfo=new ProcessStartInfo{FileName=p.adbPath,Arguments=args,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};process.Start();var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();var exited=Task.Run(()=>process.WaitForExit());if(await Task.WhenAny(exited,Task.Delay(timeout))!=exited){try{process.Kill();}catch{}throw new IOException("El teléfono no respondió. Revisa el cable y vuelve a detectar.");}string output=await stdout,error=await stderr;if(process.ExitCode!=0)throw new IOException("No se pudo completar la conexión USB. "+error.Trim());return output;}
    }
    public static async Task<List<CGMobilePhone>> Detect(CGMobilePackage p){return ParsePhones(await Adb(p,"devices -l",15000));}
    public static async Task<string> Send(CGMobilePackage p,CGMobilePhone phone){
        if(phone==null||!phone.Ready)throw new IOException("Selecciona un teléfono autorizado.");await Task.Run(()=>Verify(p));
        var current=await Detect(p);if(!current.Exists(d=>d.serial==phone.serial&&d.Ready))throw new IOException("El teléfono se desconectó o falta autorizarlo.");
        await Adb(p,"-s "+Quote(phone.serial)+" push "+Quote(p.apkPath)+" "+Quote("/sdcard/Download/Campus Glitch.apk"),120000);
        return "APK enviada a Descargas. Ábrela en el teléfono para instalar o actualizar.";
    }
    public static string ChooseDestination(){
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.SaveFilePanel("Guardar Campus Glitch",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Campus Glitch","apk");
#elif UNITY_STANDALONE_WIN
        return CGMobileWindows.SaveApk();
#else
        return null;
#endif
    }
}
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
static class CGMobileWindows {
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,CharSet=System.Runtime.InteropServices.CharSet.Unicode)]
    sealed class FileDialog {
        public int size=System.Runtime.InteropServices.Marshal.SizeOf(typeof(FileDialog));public IntPtr owner,instance;public string filter="Aplicación Android (*.apk)\0*.apk\0\0";public IntPtr customFilter;public int maxCustomFilter,filterIndex=1;public IntPtr file;public int maxFile=1024;public IntPtr fileTitle;public int maxFileTitle;public string initialDir=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),title="Guardar Campus Glitch";public int flags=0x80000|0x800|0x8|0x2;public short fileOffset,fileExtension;public string defaultExtension="apk";public IntPtr customData,hook;public string templateName;public IntPtr reserved;public int reservedInt,flagsEx;
    }
    [System.Runtime.InteropServices.DllImport("comdlg32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode,SetLastError=true)]static extern bool GetSaveFileNameW([System.Runtime.InteropServices.In,System.Runtime.InteropServices.Out] FileDialog data);
    [System.Runtime.InteropServices.DllImport("comdlg32.dll")]static extern int CommDlgExtendedError();
    public static string SaveApk(){var d=new FileDialog();d.file=System.Runtime.InteropServices.Marshal.StringToHGlobalUni("Campus Glitch.apk".PadRight(1023,'\0'));try{if(GetSaveFileNameW(d))return System.Runtime.InteropServices.Marshal.PtrToStringUni(d.file);int error=CommDlgExtendedError();if(error!=0)throw new IOException("No se pudo abrir la elección de carpeta ("+error+").");return null;}finally{System.Runtime.InteropServices.Marshal.FreeHGlobal(d.file);}}
}
#endif
}
