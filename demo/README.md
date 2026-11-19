# Working .NET 8 Clean Architecture example

The source code lives under `src/` and `tests/` at the repository root.

Run from the repository root:

```bash
dotnet restore CleanArchitecture.sln
dotnet test CleanArchitecture.sln
dotnet run --project src/CleanArchitecture.Api --urls http://localhost:5080
```

In another terminal:

```bash
curl -i -X POST http://localhost:5080/orders \
  -H "Content-Type: application/json" \
  -d '{"sku":"BOOK","unitPrice":25,"quantity":2}'
# Copy the returned id, then:
curl -i -X POST http://localhost:5080/orders/YOUR_ORDER_ID/confirm
curl http://localhost:5080/orders/YOUR_ORDER_ID
```

The domain is plain C#; the application defines repository ports; infrastructure implements an in-memory adapter; the API wires dependencies. The tests exercise business rules and check forbidden assembly references. The in-memory adapter is not production persistence.

Requires the .NET 8 SDK or a newer SDK with .NET 8 targeting support. This example has not been executed in the authoring environment; run the commands locally to verify it.
