module AppearCollapser.Command

open System
open System.Diagnostics
open System.Threading.Tasks

type CommandResult = { 
  ExitCode: int; 
  StandardOutput: string;
  StandardError: string 
}

let executeCommand executable dir args  =
  async {
    let startInfo = ProcessStartInfo()
    startInfo.FileName <- executable
    for a in args do
      startInfo.ArgumentList.Add(a)
    startInfo.WorkingDirectory <- dir
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.UseShellExecute <- false
    startInfo.CreateNoWindow <- true
    use p = new Process()
    p.StartInfo <- startInfo
    p.Start() |> ignore

    let outTask = Task.WhenAll([|
      p.StandardOutput.ReadToEndAsync();
      p.StandardError.ReadToEndAsync()
    |])

    do! p.WaitForExitAsync() |> Async.AwaitTask
    let! out = outTask |> Async.AwaitTask
    return {
      ExitCode = p.ExitCode;
      StandardOutput = out[0];
      StandardError = out[1]
    }
  }

let executeShellCommand dir command  =
  if Environment.OSVersion.Platform = PlatformID.Win32NT then
    executeCommand "cmd.exe" dir [ command ] 
  else
    executeCommand "/usr/bin/env" dir [ "-S"; "bash"; "-c"; command ] 