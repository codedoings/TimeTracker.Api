# TimeTrack.Api

# Get Started


The web api uses google to authenticate a user. It then creates a jwt token that is stored in a httponly cookie on the client.
The token is signed using asymmetric key. Use OpenSSL to generate the keys: https://slproweb.com/products/Win32OpenSSL.html

1. Start openssl command prompt
1. Run openssl genpkey -algorithm RSA -out timetrackerapi_private_key.pem -pkeyopt rsa_keygen_bits:2048
1. Run openssl rsa -pubout -in timetrackerapi_private_key.pem -out timetrackerapi_public_key.pem
1. Start powershell to create user-secrets that will be loaded into configuration on startup
  1. Go to TimeTracker.Api directory. Run dotnet user-secrets init --id="3041c690-554d-4aca-a0fb-f1e831fcdf8c"
  1. Run $publicKey = Get-Content <PATH_TO_PUBLIC_KEY_PEM_FILE> -Raw
  1. Run $privateKey = Get-Content <PATH_TO_PRIVATE_KEY_PEM_FILE> -Raw
  1. dotnet user-secrets set "AuthOptions:PublicKey" $publicKey
  1. dotnet user-secrets set "AuthOptions:PrivateKey" $privateKey

Start the api.