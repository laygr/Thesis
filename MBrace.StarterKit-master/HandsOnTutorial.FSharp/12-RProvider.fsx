let R_Installer = "http://cran.cnr.berkeley.edu/bin/windows/base/R-3.2.2-win.exe"

/// checks whether R is installed in the local computer
let isRInstalled() = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core") <> null

/// Performs R installation operation on an MBrace cluster
/// Assumes workers running with elevated privileges
let installR () = cloud {
    let installRToCurrentWorker() = local {
        if not <| isRInstalled() then
            do! Cloud.Logf "Installing R in local machine."
            use wc = new System.Net.WebClient()
            let tmp = Path.GetTempPath()
            let tmpExe = Path.Combine(tmp, Path.ChangeExtension(Path.GetRandomFileName(),".exe"))
            do! Cloud.Logf "Downloading R bits..."
            do wc.DownloadFile(Uri R_Installer, tmpExe)
            do! Cloud.Logf "Installing R..."
            let psi = new ProcessStartInfo(tmpExe, "/COMPONENTS=x64,main,translation /SILENT")
            psi.UseShellExecute <- false
            let proc = Process.Start(psi)
            proc.WaitForExit()
            if proc.ExitCode <> 0 then invalidOp "failed to install R in local context"
            do! Cloud.Logf "R installation complete."
    }

    // performs install operation for every worker in the current cluster   
    let! _ = Cloud.ParallelEverywhere(installRToCurrentWorker())
    return ()
}

/// Parallel workflow that verifies whether R is successfully installed across the cluster
let isRInstalledCloud() = cloud {
    let! results = Cloud.ParallelEverywhere (cloud { return isRInstalled ()})
    return Array.forall id results
}

// test RProvider

#I "../packages/FSharp.Data"
#I "../packages/RProvider"
#load "RProvider.fsx"


open System
open RDotNet
open RProvider
open RProvider.stats
open RProvider.graphics
open RProvider.grDevices
open RProvider.datasets

let testR() = cloud {
    let x = System.IntPtr.Size
    // basic test if RProvider works correctly
    R.mean([1;2;3;4])
    // val it : RDotNet.SymbolicExpression = [1] 2.5

    // testing graphics

    // Calculate sin using the R 'sin' function
    // (converting results to 'float') and plot it
    return
        [ for x in 0.0 .. 0.1 .. 3.14 -> 
            R.sin(x).GetValue<float>() ]

}

let task = 
    testR()
    |> cluster.CreateProcess
task.ShowInfo()
let text = task.Result