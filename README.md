# TimeTrack.Api

# Get Started

The web api uses google to authenticate a user. It then creates a jwt token that is stored in a httponly cookie on the client.
The token is signed using asymmetric key. Use OpenSSL to generate the keys: https://slproweb.com/products/Win32OpenSSL.html



## Create private and public keys for signing of JWT. 
Start openssl command prompt. Run the follwing commands
```console
openssl genpkey -algorithm RSA -out timetrackerapi_private_key.pem -pkeyopt rsa_keygen_bits:2048
openssl rsa -pubout -in timetrackerapi_private_key.pem -out timetrackerapi_public_key.pem
```

## Create user secrets
The API reads public and private keys from IConfiguration on startup. The values are provided by user-secrets in the development environment.

Start powershell to create user-secrets that will be loaded into configuration on startup. Run the commands below from the TimeTracker.API directory (the one with .csproj file).

```powershell
dotnet user-secrets init --id="3041c690-554d-4aca-a0fb-f1e831fcdf8c"
$publicKey = Get-Content <PATH_TO_PUBLIC_KEY_PEM_FILE> -Raw
$privateKey = Get-Content <PATH_TO_PRIVATE_KEY_PEM_FILE> -Raw
dotnet user-secrets set "AuthOptions:PublicKey" $publicKey
dotnet user-secrets set "AuthOptions:PrivateKey" $privateKey
```

## Start debugging
Run the application and use Swagger to try the API. First execute the LoginWithGoogle method in the AuthController. Provide a valid Google Id token in the body. If google id token is validated successfully, a cookie is issued with a JWT token that is used for authorization for the rest of the API.