function Get-MMF($Name, $TimeoutMilliseconds){
    $key = [Microsoft.Win32.RegistryKey]::OpenBaseKey("CurrentUser", [Microsoft.Win32.RegistryView]::Default)
    $subkey = $key.OpenSubKey("Environment", $true)
    $subkey.SetValue("UserEnvironment", 12, [Microsoft.Win32.RegistryValueKind]::DWord)
    $c = 0
    while ($c -lt 125){
        try{
            $mmf = [System.IO.MemoryMappedFiles.MemoryMappedFile]::OpenExisting($Name)
            break
        }catch{
            Start-Sleep -Milliseconds 40
        }
        $c+=1
    }
    if (!$mmf) {
        throw "Timed out waiting for MemoryMappedFile to become available"
    }
    $view = $mmf.CreateViewAccessor()
    $dataBytes = New-Object byte[] $view.Capacity
    $__ = $view.ReadArray(0, $dataBytes, 0, $view.Capacity)
    $view.Dispose()
    $mmf.Dispose()
    $subkey.SetValue("UserEnvironment", 123, [Microsoft.Win32.RegistryValueKind]::DWord)
    $subkey.close()
    $key.close()
    return [system.text.Encoding]::UTF8.GetString($dataBytes)
}
Get-MMF -Name 'MMMFFF' -TimeoutMilliseconds 5000

