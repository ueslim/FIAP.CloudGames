param(
  [string]$UsersBase = "http://localhost:5001",
  [string]$GamesBase = "http://localhost:5002",
  [string]$PaymentsBase = "http://localhost:5003"
)

Invoke-WebRequest "$UsersBase/swagger/v1/swagger.json" -OutFile "openapi.users.json"
Invoke-WebRequest "$GamesBase/swagger/v1/swagger.json" -OutFile "openapi.games.json"
Invoke-WebRequest "$PaymentsBase/swagger/v1/swagger.json" -OutFile "openapi.payments.json"
