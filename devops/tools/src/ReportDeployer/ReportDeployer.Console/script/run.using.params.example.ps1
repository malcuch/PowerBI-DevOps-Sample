param([string]$parametersFileName='params.example.json')


$parameters = Get-Content $parametersFileName | ConvertFrom-Json
$args = @()

foreach ($prop in $parameters.PsObject.Properties) 
{
    $args += "--$($prop.Name)";
    $args += $prop.Value;
}

&dotnet ReportDeployer.Console.dll $args