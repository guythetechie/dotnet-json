# JSON Manipulation Library for .NET

A functional-style JSON manipulation library for .NET that provides safe, composable operations for working with JSON data using the `System.Text.Json` namespace.

## Features

- **Type-safe JSON operations** - All operations return `Result<T>` types to handle errors gracefully
- **Functional composition** - Chain operations together safely without exception handling
- **Null-safe** - Handles null values explicitly through the type system
- **Built on System.Text.Json** - Uses the standard .NET JSON library under the hood
- **Immutable operations** - JSON manipulation operations are side-effect free

## Quick Start

```csharp
Result<(string Role, Uri site)> GetRole(JsonObject jsonObject) =>
    from role in jsonObject.GetStringProperty("role")
    from site in jsonObject.GetAbsoluteUriProperty("site")
    select (role, site);

Result<ImmutableArray<(string Role, Uri site)>> GetRoles(JsonArray jsonArray, CancellationToken cancellationToken) =>
    from roleJsonObjects in jsonArray.GetJsonObjects()
    // Loop through each JSON object. If all are successful, return a successful result
    // If one or more fail, return a list of failures.
    from roles in roleJsonObjects.Traverse(GetRole, cancellationToken)
    // Return a list of roles
    select roles;

async Task<Result<ImmutableArray<(string Role, Uri site)>>> GetRoles(HttpClient client, Uri uri, CancellationToken cancellationToken)
{
    var data = await client.GetBinaryData(uri, cancellationToken);
    
    return from node in JsonNodeModule.From(binaryData) // Parse binary data into JsonNode
           from contentJsonObject in node.AsJsonObject() // Converts JsonNode to JsonObject
           from rolesJsonArray in contentJsonObject.GetJsonArrayProperty("roles") // Get roles JSON array property
           from roles in GetRoles(rolesJsonArray) // Get roles from JSON array
           select roles;
}

// Successful example
// {
//   "roles": [
//     {
//       "role": "admin",
//       "site": "https://admin.example.com"
//     },
//     {
//       "role": "user",
//       "site": "https://app.example.com"
//     },
//     {
//       "role": "moderator",
//       "site": "https://community.example.com"
//     }
//   ]
// }
var successResult = GetRoles(client, goodUri, cancellationToken)

// Failing example
// {
//   "roles": [
//     {
//       "role": "admin",
//       "site": "b"
//     },
//     {
//       "role": 1,
//       "site": "https://anothersite.com"
//     },
//     {
//       "role": "moderator",
//       "site": "https://community.example.com"
//     }
//   ]
// }
var failingResult = GetRoles(client, goodUri, cancellationToken)
// Returns result with errors ["Property 'site' is invalid. JSON value is not an absolute URI.",
//                             "Property 'role' is invalid. JSON value is not an integer."]
```

## API Reference

| Class | Method | Description |
|-------|--------|-------------|
| JsonNodeModule | [`From(BinaryData?, JsonNodeOptions?)`](#frombinarydata-data-jsonnodeoptions-options--default) | Parses a `JsonNode` from binary data |
| JsonNodeModule | [`From(Stream?, JsonNodeOptions?, JsonDocumentOptions, CancellationToken)`](#fromstream-data-jsonnodeoptions-nodeoptions--default-jsondocumentoptions-documentoptions--default-cancellationtoken-cancellationtoken--default) | Asynchronously parses a `JsonNode` from a stream |
| JsonNodeModule | [`AsJsonObject(this JsonNode?)`](#asjsonobjectthis-jsonnode-node) | Converts a `JsonNode` to a `JsonObject` |
| JsonNodeModule | [`AsJsonArray(this JsonNode?)`](#asjsonarraythis-jsonnode-node) | Converts a `JsonNode` to a `JsonArray` |
| JsonNodeModule | [`AsJsonValue(this JsonNode?)`](#asjsonvaluethis-jsonnode-node) | Converts a `JsonNode` to a `JsonValue` |
| JsonNodeModule | [`Deserialize<T>(BinaryData?, JsonSerializerOptions?)`](#deserializetbinarydata-data-jsonserializeroptions-options--default) | Deserializes binary data to a specific type |
| JsonNodeModule | [`ToStream(JsonNode, JsonSerializerOptions?)`](#tostreamjsonnode-node-jsonserializeroptions-options--default) | Converts a `JsonNode` to a stream |
| JsonObjectModule | [`GetProperty(this JsonObject?, string)`](#getpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a property from a JSON object |
| JsonObjectModule | [`GetOptionalProperty(this JsonObject?, string)`](#getoptionalpropertythis-jsonobject-jsonobject-string-propertyname) | Gets an optional property from a JSON object |
| JsonObjectModule | [`GetStringProperty(this JsonObject?, string)`](#getstringpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a string property from a JSON object |
| JsonObjectModule | [`GetIntProperty(this JsonObject?, string)`](#getintpropertythis-jsonobject-jsonobject-string-propertyname) | Gets an integer property from a JSON object |
| JsonObjectModule | [`GetBoolProperty(this JsonObject?, string)`](#getboolpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a boolean property from a JSON object |
| JsonObjectModule | [`GetGuidProperty(this JsonObject?, string)`](#getguidpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a GUID property from a JSON object |
| JsonObjectModule | [`GetAbsoluteUriProperty(this JsonObject?, string)`](#getabsoluteuripropertythis-jsonobject-jsonobject-string-propertyname) | Gets an absolute URI property from a JSON object |
| JsonObjectModule | [`SetProperty(this JsonObject, string, JsonNode?)`](#setpropertythis-jsonobject-jsonobject-string-propertyname-jsonnode-propertyvalue) | Sets a property on a JSON object |
| JsonValueModule | [`AsString(this JsonValue?)`](#asstringthis-jsonvalue-jsonvalue) | Converts a JSON value to a string |
| JsonValueModule | [`AsInt(this JsonValue?)`](#asintthis-jsonvalue-jsonvalue) | Converts a JSON value to an integer |
| JsonValueModule | [`AsBool(this JsonValue?)`](#asboolthis-jsonvalue-jsonvalue) | Converts a JSON value to a boolean |
| JsonValueModule | [`AsGuid(this JsonValue?)`](#asguidthis-jsonvalue-jsonvalue) | Converts a JSON value to a GUID |
| JsonValueModule | [`AsAbsoluteUri(this JsonValue?)`](#asabsoluteurithis-jsonvalue-jsonvalue) | Converts a JSON value to an absolute URI |
| JsonArrayModule | [`ToJsonArray(this IEnumerable<JsonNode?>)`](#tojsonarraythis-ienumerablejsonnode-nodes) | Creates a JSON array from an enumerable of JSON nodes |
| JsonArrayModule | [`ToJsonArray(this IAsyncEnumerable<JsonNode?>, CancellationToken)`](#tojsonarraythis-iasyncenumerablejsonnode-nodes-cancellationtoken-cancellationtoken) | Asynchronously creates a JSON array from an async enumerable |
| JsonArrayModule | [`GetJsonObjects(this JsonArray)`](#getjsonobjectsthis-jsonarray-jsonarray) | Gets all elements as JSON objects from a JSON array |
| JsonArrayModule | [`GetJsonArrays(this JsonArray)`](#getjsonarraysthis-jsonarray-jsonarray) | Gets all elements as JSON arrays from a JSON array |
| JsonArrayModule | [`GetJsonValues(this JsonArray)`](#getjsonvaluesthis-jsonarray-jsonarray) | Gets all elements as JSON values from a JSON array |

### JsonNodeModule

Core functionality for parsing and converting JSON nodes.

#### `From(BinaryData? data, JsonNodeOptions? options = default)`

Parses a `JsonNode` from binary data.

```csharp
var data = BinaryData.FromString("""{"hello": "world"}""");
var result = JsonNodeModule.From(data);
// Returns: Success(JsonNode)

var invalidData = BinaryData.FromString("invalid json");
var errorResult = JsonNodeModule.From(invalidData);
// Returns: Error("...")
```

#### `From(Stream? data, JsonNodeOptions? nodeOptions = default, JsonDocumentOptions documentOptions = default, CancellationToken cancellationToken = default)`

Asynchronously parses a `JsonNode` from a stream.

```csharp
using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""{"data": "value"}"""));
var result = await JsonNodeModule.From(stream);
// Returns: Success(JsonNode)
```

#### `AsJsonObject(this JsonNode? node)`

Converts a `JsonNode` to a `JsonObject`.

```csharp
var node = JsonNode.Parse("""{"key": "value"}""");
var result = node.AsJsonObject();
// Returns: Success(JsonObject)

var arrayNode = JsonNode.Parse("""["item"]""");
var errorResult = arrayNode.AsJsonObject();
// Returns: Error("JSON node is not a JSON object.")
```

#### `AsJsonArray(this JsonNode? node)`

Converts a `JsonNode` to a `JsonArray`.

```csharp
var node = JsonNode.Parse("""[1, 2, 3]""");
var result = node.AsJsonArray();
// Returns: Success(JsonArray)
```

#### `AsJsonValue(this JsonNode? node)`

Converts a `JsonNode` to a `JsonValue`.

```csharp
var node = JsonNode.Parse(""""hello"""");
var result = node.AsJsonValue();
// Returns: Success(JsonValue)
```

#### `Deserialize<T>(BinaryData? data, JsonSerializerOptions? options = default)`

Deserializes binary data to a specific type.

```csharp
var data = BinaryData.FromString("""{"name": "John"}""");
var result = JsonNodeModule.Deserialize<JsonObject>(data);
// Returns: Success(JsonObject)
```

#### `ToStream(JsonNode node, JsonSerializerOptions? options = default)`

Converts a `JsonNode` to a stream.

```csharp
var node = JsonNode.Parse("""{"data": "value"}""");
var stream = JsonNodeModule.ToStream(node);
// Returns: Stream containing JSON data
```

### JsonObjectModule

Operations for working with JSON objects and their properties.

#### `GetProperty(this JsonObject? jsonObject, string propertyName)`

Gets a property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"name": "John"}""").AsObject();
var result = obj.GetProperty("name");
// Returns: Success(JsonNode)

var missingResult = obj.GetProperty("missing");
// Returns: Error("JSON object does not have a property named 'missing'.")
```

#### `GetOptionalProperty(this JsonObject? jsonObject, string propertyName)`

Gets an optional property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"name": "John"}""").AsObject();
var result = obj.GetOptionalProperty("name");
// Returns: Some(JsonNode)

var missingResult = obj.GetOptionalProperty("missing");
// Returns: None
```

#### `GetStringProperty(this JsonObject? jsonObject, string propertyName)`

Gets a string property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"name": "John"}""").AsObject();
var result = obj.GetStringProperty("name");
// Returns: Success("John")
```

#### `GetIntProperty(this JsonObject? jsonObject, string propertyName)`

Gets an integer property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"age": 30}""").AsObject();
var result = obj.GetIntProperty("age");
// Returns: Success(30)
```

#### `GetBoolProperty(this JsonObject? jsonObject, string propertyName)`

Gets a boolean property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"active": true}""").AsObject();
var result = obj.GetBoolProperty("active");
// Returns: Success(true)
```

#### `GetGuidProperty(this JsonObject? jsonObject, string propertyName)`

Gets a GUID property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"id": "123e4567-e89b-12d3-a456-426614174000"}""").AsObject();
var result = obj.GetGuidProperty("id");
// Returns: Success(Guid)
```

#### `GetAbsoluteUriProperty(this JsonObject? jsonObject, string propertyName)`

Gets an absolute URI property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"url": "https://example.com"}""").AsObject();
var result = obj.GetAbsoluteUriProperty("url");
// Returns: Success(Uri)
```

#### `SetProperty(this JsonObject jsonObject, string propertyName, JsonNode? propertyValue)`

Sets a property on a JSON object.

```csharp
var obj = new JsonObject();
var updated = obj.SetProperty("name", JsonValue.Create("John"));
// Returns: JsonObject with "name" property set
```

### JsonValueModule

Type conversion operations for JSON values.

#### `AsString(this JsonValue? jsonValue)`

Converts a JSON value to a string.

```csharp
var value = JsonValue.Create("hello");
var result = value.AsString();
// Returns: Success("hello")

var numberValue = JsonValue.Create(123);
var errorResult = numberValue.AsString();
// Returns: Error("JSON value is not a string.")
```

#### `AsInt(this JsonValue? jsonValue)`

Converts a JSON value to an integer.

```csharp
var value = JsonValue.Create(42);
var result = value.AsInt();
// Returns: Success(42)
```

#### `AsBool(this JsonValue? jsonValue)`

Converts a JSON value to a boolean.

```csharp
var value = JsonValue.Create(true);
var result = value.AsBool();
// Returns: Success(true)
```

#### `AsGuid(this JsonValue? jsonValue)`

Converts a JSON value to a GUID.

```csharp
var value = JsonValue.Create("123e4567-e89b-12d3-a456-426614174000");
var result = value.AsGuid();
// Returns: Success(Guid)
```

#### `AsAbsoluteUri(this JsonValue? jsonValue)`

Converts a JSON value to an absolute URI.

```csharp
var value = JsonValue.Create("https://example.com");
var result = value.AsAbsoluteUri();
// Returns: Success(Uri)
```

### JsonArrayModule

Operations for working with JSON arrays and their elements.

#### `ToJsonArray(this IEnumerable<JsonNode?> nodes)`

Creates a JSON array from an enumerable of JSON nodes.

```csharp
var nodes = new[] { JsonValue.Create(1), JsonValue.Create(2) };
var array = nodes.ToJsonArray();
// Returns: JsonArray containing [1, 2]
```

#### `ToJsonArray(this IAsyncEnumerable<JsonNode?> nodes, CancellationToken cancellationToken)`

Asynchronously creates a JSON array from an async enumerable of JSON nodes.

```csharp
async IAsyncEnumerable<JsonNode?> GetNodesAsync()
{
    yield return JsonValue.Create(1);
    yield return JsonValue.Create(2);
}

var array = await GetNodesAsync().ToJsonArray(CancellationToken.None);
// Returns: JsonArray containing [1, 2]
```

#### `GetJsonObjects(this JsonArray jsonArray)`

Gets all elements as JSON objects from a JSON array.

```csharp
var array = JsonNode.Parse("""[{"id": 1}, {"id": 2}]""").AsArray();
var result = array.GetJsonObjects();
// Returns: Success(ImmutableArray<JsonObject>)
```

#### `GetJsonArrays(this JsonArray jsonArray)`

Gets all elements as JSON arrays from a JSON array.

```csharp
var array = JsonNode.Parse("""[[1, 2], [3, 4]]""").AsArray();
var result = array.GetJsonArrays();
// Returns: Success(ImmutableArray<JsonArray>)
```

#### `GetJsonValues(this JsonArray jsonArray)`

Gets all elements as JSON values from a JSON array.

```csharp
var array = JsonNode.Parse("""[1, 2, 3]""").AsArray();
var result = array.GetJsonValues();
// Returns: Success(ImmutableArray<JsonValue>)
```

## License

This project is licensed under the MIT License.
