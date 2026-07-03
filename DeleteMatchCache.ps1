$HostName = "das-prd-shared-rds.redis.cache.windows.net"
$Port = 6380
$AccessKey = ""

$GovUkIdentifiers = @(
"urn:fdc:gov.uk:2022:Ckt4NE-SdYOgv22lyGm5Z5KLFTLQa_xKuFE5ZaI6GKQ",
"urn:fdc:gov.uk:2022:q9rTiFpqsCNZaGGyTEej5d8LPQBMhV1G_svbWkapwHs",
"urn:fdc:gov.uk:2022:qzdIhhDEJakPck6FW6w7OG8z2UUk1fq933UxY7FnXww",
"urn:fdc:gov.uk:2022:QeLHAZpq91tN_2G3foknhT34qYqnsT0hykaWjnKjnAo",
"urn:fdc:gov.uk:2022:QZG6YqSe84wUkooWODD09SugRVR3vyWC-seMfOKkaxw",
"urn:fdc:gov.uk:2022:f0OQiGpSVL7XKinC9OGBvJR5v1MkEFfxYrkUluj_-V4",
"urn:fdc:gov.uk:2022:DUobbdHk9it1K8tO_irY94jhyjD4IAD6V91itlIb2rU",
"urn:fdc:gov.uk:2022:4Qi7u0IbLoNW-0r7V6DvJVlN5qDiWmuc-Yd3XYgf1Fo",
"urn:fdc:gov.uk:2022:7-D3lY26nNS_P_vIogmTVLTr_dusvZL3-xux-5e0uOI",
"urn:fdc:gov.uk:2022:LN6-CMMTQDvsEnxC9cVqLaG9grJyglRGscR5a-44Jmo",
"urn:fdc:gov.uk:2022:jUZrrLg1xPR2jcNok9IFotPOEwgy28rrvJYAFLJK2tI",
"urn:fdc:gov.uk:2022:ws5LaPwDYOSNLycpOrVM_pC7oUkN3KG8_fj4JK56dUA",
"urn:fdc:gov.uk:2022:N5nQ8-edbBQQ5yJXaITgiAfGPNG3gqge4HpANBQSdoU",
"urn:fdc:gov.uk:2022:priSqcCFqUpGlXfyYVkeKeZ4s-P757ivsESO8oJyqWw",
"urn:fdc:gov.uk:2022:8P-qnKONpcsFTunguHVANuUWalkg1JanGwqNlx31nmI",
"urn:fdc:gov.uk:2022:JSfh5Tay_pSCU3DsvY39Akn54CNVga3uX1qERtO5-5o",
"urn:fdc:gov.uk:2022:GocSS6X5ZUHkuiGqZhJ37Gi_H4LOiLakhWWVNz95Yt0",
"urn:fdc:gov.uk:2022:7yN1ILNmI6q99ABYOT0Ja_8pprdH9DG9UMbmR_s_BX4",
"urn:fdc:gov.uk:2022:hW4SAqGFJPu385yQHetg1LN1DQVeAtS79BZOWUZWCTU",
"urn:fdc:gov.uk:2022:OKQc2S4CarwWMCYHAUhZ1W--9qWbw76puPnnw0durC0",
"urn:fdc:gov.uk:2022:yXUz-TACclBZkrSj40K4OK1VdOg_BsJgWre5v3SCuI0",
"urn:fdc:gov.uk:2022:cqJ_seQkzRonyKlECC24pY2qLYVo_3TgNFUi7tqyDnU",
"urn:fdc:gov.uk:2022:tzCZK9TbBI9NegTGFFYMaxWKFiFJNL-OC6niWYjKcoc",
"urn:fdc:gov.uk:2022:xd8eDB0EAE_qCzBG3VEK6cPBivHKuyK3sYv72aBNQbU",
"urn:fdc:gov.uk:2022:ChvuhMUnfkHrMa7GQ9xANd18sbXa2o1laCsefDzG1Lk",
"urn:fdc:gov.uk:2022:Nhck5PDD0mevww8x6zKW0Hc7PMV_fnBEy72niC7mswY",
"urn:fdc:gov.uk:2022:6h9_K-uNKaqXKKQP84xsguCYy36Lbf46ZPJ-MBRcp8Q",
"urn:fdc:gov.uk:2022:HWG1qI3Qj_G7H9KPXiZxKYLpbRrS07flchpFcil95bk",
"urn:fdc:gov.uk:2022:mbh4UJunCemrY1_dsPIPPxiZ05TO2TO51E0GkEUvPOI",
"urn:fdc:gov.uk:2022:E3eHj_a2PAYGmNLnnGZsbDA0fgGHux1LEiO5dmByFXk",
"urn:fdc:gov.uk:2022:ERzMc6SoK2h2pgnYqeURvR78OvjSIT44XZr5z5X04zc",
"urn:fdc:gov.uk:2022:BW7gTUOszWGIgvnBz4u4h99oYs1oPFxzssbk-ZKPOZE",
"urn:fdc:gov.uk:2022:fJaWhFyfl2JcxWcnufFSpdI66vBw1Gw0DSU1sxLUeRU",
"urn:fdc:gov.uk:2022:Tnq-LrLCbAN9N1vi2QYZBfRuUCg-qD7zGsc8HFWPNKU",
"urn:fdc:gov.uk:2022:aJxhzdhquuLlc3ao6WNVtVuZrqQwDjGLOTcAOBiN9YE",
"urn:fdc:gov.uk:2022:YoiJwYLRzAwa7E2FCEXIAncYQ0oUOMBffCmCMhgWdvw",
"urn:fdc:gov.uk:2022:VscQhwjlDGrQVgspaz0U71ZmpUTfPNYm3u6uXfoYinc",
"urn:fdc:gov.uk:2022:o6b_5bTBRbaoYEdTto-9rNjFkBzXGiC-ntPjD6bF1e4",
"urn:fdc:gov.uk:2022:VnRIq5dAJMUZwIAWuwfXLGSe7aaoXIupNtfVnRGNruo",
"urn:fdc:gov.uk:2022:Zo6FsCjLooiOCeKoU6apdK4U2C6R8gb0laz9o1VpmFM",
"urn:fdc:gov.uk:2022:IuOJkeVmT73NEqfunoAENKIE7OLzK_xhe6TClBREIuc",
"urn:fdc:gov.uk:2022:gdiFIFjNu19yKiVwVd9kzwSUA8vi3fn8mW6DTfDJYFE",
"urn:fdc:gov.uk:2022:IXE4abJSBfplDXDyl0zZ9GmSZumYF7WYmAX9ICNMwQg",
"urn:fdc:gov.uk:2022:exhWodL8yCwXi6nOUObE0luinBwqrlEjphUPHFh9dYo",
"urn:fdc:gov.uk:2022:gADr_NHD3kceS3fjyNkRAkJHgZzithPonIZCJMkrGLg",
"urn:fdc:gov.uk:2022:55xMm0JjDenZki4WbzbwR9TCXhc-wKL9nYKZI74s4M0",
"urn:fdc:gov.uk:2022:Sww-_lmLGw38lKihfbUUpXE2e4cAvkJORW5wU6xooBc",
"urn:fdc:gov.uk:2022:l5Dvt9eAKz0_RxytPkxgWvDUrEYGkpvhP1ltn0wSMas",
"urn:fdc:gov.uk:2022:yStjB9yxHEL5HaHCIodm8YHUzeyuQdkRy1voeT6WptA",
"urn:fdc:gov.uk:2022:-uZG2cJEBYK9ZkWAn9ivi6a_YpeHHu6HuMwu35CqeyY",
"urn:fdc:gov.uk:2022:TSSNgreo3O0PP-YvRoom_Q4TEQ4wtKgBzR4akPe8AMM",
"urn:fdc:gov.uk:2022:T1wDoy-_-x7GoKHmOZ7Syt_ZD1WjHfd94Nllv7UENmQ",
"urn:fdc:gov.uk:2022:LRvYLdaU7gZajQ4mDt5ECQsHUUmOzX-jFCKRdv1pWtQ",
"urn:fdc:gov.uk:2022:YoLa74SCGkD-MhSBc9EtByxwsRdCTyR8NEh5_a-T5yA",
"urn:fdc:gov.uk:2022:Nvbn_4Ou9BXZsY5L6i9080E_Iq_xHQRe1knaSPkZjF4",
"urn:fdc:gov.uk:2022:pAfJ_LQaS1WMmPwWBqDLLx9tTpTgV9ZbSdVtlXDPMd0"
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