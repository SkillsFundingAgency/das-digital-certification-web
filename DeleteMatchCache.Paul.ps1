$HostName = "das-prd-shared-rds.redis.cache.windows.net"
$Port = 6380
$AccessKey = ""

$GovUkIdentifiers = @(
"urn:fdc:gov.uk:2022:cvDWNFBWjnSuoMwZWN59hmbnSkwPdbue8xdD3CTjoDk"
)

function New-RedisCommand {
    param([string[]]$Parts)

    $command = "*" + $Parts.Count + "`r`n"

    foreach ($part in $Parts) {
        $byteCount = [System.Text.Encoding]::UTF8.GetByteCount($part)
        $command += '$' + $byteCount + "`r`n" + $part + "`r`n"
    }

    return $command
}

function Send-RedisCommand {
    param(
        [System.Net.Security.SslStream]$Stream,
        [string[]]$Parts
    )

    $command = New-RedisCommand -Parts $Parts
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($command)

    $Stream.Write($bytes, 0, $bytes.Length)
    $Stream.Flush()

    Start-Sleep -Milliseconds 100

    $buffer = New-Object byte[] 8192
    $read = $Stream.Read($buffer, 0, $buffer.Length)

    return [System.Text.Encoding]::UTF8.GetString($buffer, 0, $read).Trim()
}

function Convert-RedisIntegerResponse {
    param([string]$Response)

    if ($Response.StartsWith(":")) {
        return [int]$Response.Substring(1)
    }

    return $Response
}

Write-Host "Connecting to $HostName on port $Port..."

$client = [System.Net.Sockets.TcpClient]::new()
$client.Connect($HostName, $Port)

$sslStream = [System.Net.Security.SslStream]::new($client.GetStream(), $false)
$sslStream.AuthenticateAsClient($HostName)

Write-Host "TLS connection established."

$authResponse = Send-RedisCommand -Stream $sslStream -Parts @("AUTH", $AccessKey)

if ($authResponse -ne "+OK") {
    Write-Host "AUTH failed: $authResponse"
    $sslStream.Dispose()
    $client.Dispose()
    exit 1
}

Write-Host "AUTH successful."

$pingResponse = Send-RedisCommand -Stream $sslStream -Parts @("PING")

if ($pingResponse -ne "+PONG") {
    Write-Host "PING failed: $pingResponse"
    $sslStream.Dispose()
    $client.Dispose()
    exit 1
}

Write-Host "PING successful."
Write-Host "----"

foreach ($Identifier in $GovUkIdentifiers) {
    $MatchesKey = "DigitalCertificates:Matches:$Identifier"

    Write-Host "Checking key for identifier:"
    Write-Host $Identifier
    Write-Host ""
    Write-Host "Redis key:"
    Write-Host $MatchesKey
    Write-Host ""

    $existsResponse = Send-RedisCommand -Stream $sslStream -Parts @("EXISTS", $MatchesKey)
    $exists = Convert-RedisIntegerResponse $existsResponse

    Write-Host "EXISTS before delete:"
    Write-Host $exists

    $ttlResponse = Send-RedisCommand -Stream $sslStream -Parts @("TTL", $MatchesKey)
    $ttl = Convert-RedisIntegerResponse $ttlResponse

    Write-Host "TTL before delete:"
    Write-Host $ttl

    if ($ttl -eq -2) {
        Write-Host "TTL meaning: key does not exist"
    }
    elseif ($ttl -eq -1) {
        Write-Host "TTL meaning: key exists but has no expiry"
    }
    else {
        Write-Host "TTL meaning: key expires in $ttl seconds"
    }

    if ($exists -eq 1) {
        Write-Host ""
        Write-Host "Deleting key..."

        $deleteResponse = Send-RedisCommand -Stream $sslStream -Parts @("DEL", $MatchesKey)
        $deleted = Convert-RedisIntegerResponse $deleteResponse

        Write-Host "DEL result:"
        Write-Host $deleted
    }
    else {
        Write-Host ""
        Write-Host "Skipping delete because key does not exist."
    }

    $existsAfterDeleteResponse = Send-RedisCommand -Stream $sslStream -Parts @("EXISTS", $MatchesKey)
    $existsAfterDelete = Convert-RedisIntegerResponse $existsAfterDeleteResponse

    Write-Host ""
    Write-Host "EXISTS after delete:"
    Write-Host $existsAfterDelete

    Write-Host "----"
}
$sslStream.Dispose()
$client.Dispose()

Write-Host "Done."