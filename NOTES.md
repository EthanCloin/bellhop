[.NET Authentication/Authorization Fundamentals](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-10.0)

# Connecting to db

runs on localhost:5432 database security user postgres so connect via
`docker exec -it bellhop-db-1 psql -U postgres -d security`
can also use dbeaver and provide the localhost url + database=security + user=postgres
_not secure this is for local only dev_
