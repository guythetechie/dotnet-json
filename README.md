# JSON Manipulation Library for .NET

A functional-style JSON manipulation library for .NET that provides safe, composable operations for working with JSON data using the `System.Text.Json` namespace.

## Example

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

async Task<Result<ImmutableArray<(string Role, Uri Site)>>> GetRoles(HttpClient client, Uri uri, CancellationToken cancellationToken)
{
    var data = await client.GetBinaryData(uri, cancellationToken);
    
    return from node in JsonNodeModule.From(binaryData) // Parse binary data into JsonNode
           from contentJsonObject in node.AsJsonObject() // Converts JsonNode to JsonObject
           from rolesJsonArray in contentJsonObject.GetJsonArrayProperty("roles") // Get roles JSON array property
           from roles in GetRoles(rolesJsonArray) // Get roles from JSON array
           select roles;
}

// Successful response
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

// Returns Result with Success [(Role: "admin", Site: "https://admin.example.com"),
//                              (Role: "user", Site: "https://app.example.com")
//                              (Role: "moderator", Site: "https://community.example.com)]
var successResult = GetRoles(client, goodUri, cancellationToken)

// Invalid response
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

// Returns result with errors ["Property 'site' is invalid. JSON value is not an absolute URI.",
//                             "Property 'role' is invalid. JSON value is not an integer."]
var failingResult = GetRoles(client, goodUri, cancellationToken)
```

## API Reference

| Class | Method | Description |
|-------|--------|-------------|
| JsonNodeModule | [`From(BinaryData?, JsonNodeOptions?)`](#frombinarydata-data-jsonnodeoptions-options--default) | Parses a `JsonNode` from binary data |
| JsonNodeModule | [`From(Stream?, JsonNodeOptions?, JsonDocumentOptions, CancellationToken)`](#fromstream-data-jsonnodeoptions-nodeoptions--default-jsondocumentoptions-documentoptions--default-cancellationtoken-cancellationtoken--default) | Asynchronously parses a `JsonNode` from a stream |
| JsonNodeModule | [`From<T>(T?, JsonSerializerOptions?)`](#fromtt-data-jsonserializeroptions-options--default) | Serializes an object to a `JsonNode` |
| JsonNodeModule | [`To<T>(JsonNode?, JsonSerializerOptions?)`](#tothis-jsonnode-jsonserializeroptions-options--default) | Deserializes a `JsonNode` into a strongly-typed object |
| JsonNodeModule | [`AsJsonObject(this JsonNode?)`](#asjsonobjectthis-jsonnode-node) | Converts a `JsonNode` to a `JsonObject` |
| JsonNodeModule | [`AsJsonArray(this JsonNode?)`](#asjsonarraythis-jsonnode-node) | Converts a `JsonNode` to a `JsonArray` |
| JsonNodeModule | [`AsJsonValue(this JsonNode?)`](#asjsonvaluethis-jsonnode-node) | Converts a `JsonNode` to a `JsonValue` |
| JsonNodeModule | [`Deserialize<T>(BinaryData?, JsonSerializerOptions?)`](#deserializetbinarydata-data-jsonserializeroptions-options--default) | Deserializes binary data to a specific type |
| JsonNodeModule | [`ToStream(JsonNode, JsonSerializerOptions?)`](#tostreamjsonnode-node-jsonserializeroptions-options--default) | Converts a `JsonNode` to a stream |
| JsonObjectModule | [`GetProperty(this JsonObject?, string)`](#getpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a property from a JSON object |
| JsonObjectModule | [`GetProperty<T>(this JsonObject?, string, Func<JsonNode, Result<T>>)`](#getpropertytthis-jsonobject-jsonobject-string-propertyname-funcjsonnode-resultt-selector) | Gets and transforms a property from a JSON object using a selector function |
| JsonObjectModule | [`GetOptionalProperty(this JsonObject?, string)`](#getoptionalpropertythis-jsonobject-jsonobject-string-propertyname) | Gets an optional property from a JSON object |
| JsonObjectModule | [`GetJsonObjectProperty(this JsonObject?, string)`](#getjsonobjectpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a JSON object property from a JSON object |
| JsonObjectModule | [`GetJsonArrayProperty(this JsonObject?, string)`](#getjsonarraypropertythis-jsonobject-jsonobject-string-propertyname) | Gets a JSON array property from a JSON object |
| JsonObjectModule | [`GetJsonValueProperty(this JsonObject?, string)`](#getjsonvaluepropertythis-jsonobject-jsonobject-string-propertyname) | Gets a JSON value property from a JSON object |
| JsonObjectModule | [`GetStringProperty(this JsonObject?, string)`](#getstringpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a string property from a JSON object |
| JsonObjectModule | [`GetIntProperty(this JsonObject?, string)`](#getintpropertythis-jsonobject-jsonobject-string-propertyname) | Gets an integer property from a JSON object |
| JsonObjectModule | [`GetBoolProperty(this JsonObject?, string)`](#getboolpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a boolean property from a JSON object |
| JsonObjectModule | [`GetGuidProperty(this JsonObject?, string)`](#getguidpropertythis-jsonobject-jsonobject-string-propertyname) | Gets a GUID property from a JSON object |
| JsonObjectModule | [`GetAbsoluteUriProperty(this JsonObject?, string)`](#getabsoluteuripropertythis-jsonobject-jsonobject-string-propertyname) | Gets an absolute URI property from a JSON object |
| JsonObjectModule | [`SetProperty(this JsonObject, string, JsonNode?, bool)`](#setpropertythis-jsonobject-jsonobject-string-propertyname-jsonnode-propertyvalue-bool-mutateoriginal--false) | Sets a property on a JSON object |
| JsonObjectModule | [`RemoveProperty(this JsonObject, string, bool)`](#removepropertythis-jsonobject-jsonobject-string-propertyname-bool-mutateoriginal--false) | Removes a property from a JSON object |
| JsonObjectModule | [`From(BinaryData?, JsonSerializerOptions?)`](#frombinarydata-data-jsonserializeroptions-options--default-1) | Converts binary data to a JSON object |
| JsonValueModule | [`AsString(this JsonValue?)`](#asstringthis-jsonvalue-jsonvalue) | Converts a JSON value to a string |
| JsonValueModule | [`AsInt(this JsonValue?)`](#asintthis-jsonvalue-jsonvalue) | Converts a JSON value to an integer |
| JsonValueModule | [`AsBool(this JsonValue?)`](#asboolthis-jsonvalue-jsonvalue) | Converts a JSON value to a boolean |
| JsonValueModule | [`AsGuid(this JsonValue?)`](#asguidthis-jsonvalue-jsonvalue) | Converts a JSON value to a GUID |
| JsonValueModule | [`AsAbsoluteUri(this JsonValue?)`](#asabsoluteurithis-jsonvalue-jsonvalue) | Converts a JSON value to an absolute URI |
| JsonArrayModule | [`ToJsonArray(this IEnumerable<JsonNode?>)`](#tojsonarraythis-ienumerablejsonnode-nodes) | Creates a JSON array from an enumerable of JSON nodes |
| JsonArrayModule | [`ToJsonArray(this IAsyncEnumerable<JsonNode?>, CancellationToken)`](#tojsonarraythis-iasyncenumerablejsonnode-nodes-cancellationtoken-cancellationtoken) | Asynchronously creates a JSON array from an async enumerable |
| JsonArrayModule | [`GetElements<T>(this JsonArray, Func<JsonNode?, Result<T>>, Func<int, Error>)`](#getelementstthis-jsonarray-jsonarray-funcjsonnode-resultt-selector-funcint-error-errorfromindex) | Extracts elements from a JSON array using a selector function |
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

#### `From<T>(T? data, JsonSerializerOptions? options = default)`

Serializes an object to a `JsonNode`.

```csharp
var person = new { Name = "John", Age = 30 };
var result = JsonNodeModule.From(person);
// Returns: Success(JsonNode representing the object)

var nullData = (object?)null;
var errorResult = JsonNodeModule.From(nullData);
// Returns: Error("Serialization returned a null result.")
```

#### `To<T>(JsonNode? node, JsonSerializerOptions? options = default)`

Deserializes a `JsonNode` into a strongly-typed object.

```csharp
var node = JsonNode.Parse("""{"Name": "John", "Age": 30}""");
var result = JsonNodeModule.To<Person>(node);
// Returns: Success(Person instance)

var invalidNode = JsonNode.Parse("""{"InvalidProperty": "value"}""");
var errorResult = JsonNodeModule.To<Person>(invalidNode);
// Returns: Error with deserialization details
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

#### `GetProperty<T>(this JsonObject? jsonObject, string propertyName, Func<JsonNode, Result<T>> selector)`

Gets and transforms a property from a JSON object using a selector function. This method provides enhanced error context by including the property name in error messages.

```csharp
var obj = JsonNode.Parse("""{"count": "42"}""").AsObject();

// Transform string to integer
var result = obj.GetProperty("count", node => node.AsJsonValue().Bind(v => v.AsInt()));
// Returns: Success(42)

var invalidObj = JsonNode.Parse("""{"count": "invalid"}""").AsObject();
var errorResult = invalidObj.GetProperty("count", node => node.AsJsonValue().Bind(v => v.AsInt()));
// Returns: Error("Property 'count' is invalid. JSON value is not an integer.")

// Custom transformation
var upperResult = obj.GetProperty("name", node => node.AsJsonValue().Bind(v => v.AsString().Map(s => s.ToUpper())));
```

#### `GetJsonObjectProperty(this JsonObject? jsonObject, string propertyName)`

Gets a JSON object property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"user": {"name": "John", "age": 30}}""").AsObject();
var result = obj.GetJsonObjectProperty("user");
// Returns: Success(JsonObject)

var stringProp = JsonNode.Parse("""{"name": "John"}""").AsObject();
var errorResult = stringProp.GetJsonObjectProperty("name");
// Returns: Error("Property 'name' is invalid. JSON node is not a JSON object.")
```

#### `GetJsonArrayProperty(this JsonObject? jsonObject, string propertyName)`

Gets a JSON array property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"items": [1, 2, 3]}""").AsObject();
var result = obj.GetJsonArrayProperty("items");
// Returns: Success(JsonArray)

var stringProp = JsonNode.Parse("""{"name": "John"}""").AsObject();
var errorResult = stringProp.GetJsonArrayProperty("name");
// Returns: Error("Property 'name' is invalid. JSON node is not a JSON array.")
```

#### `GetJsonValueProperty(this JsonObject? jsonObject, string propertyName)`

Gets a JSON value property from a JSON object.

```csharp
var obj = JsonNode.Parse("""{"count": 42}""").AsObject();
var result = obj.GetJsonValueProperty("count");
// Returns: Success(JsonValue)

var objProp = JsonNode.Parse("""{"user": {"name": "John"}}""").AsObject();
var errorResult = objProp.GetJsonValueProperty("user");
// Returns: Error("Property 'user' is invalid. JSON node is not a JSON value.")
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

#### `SetProperty(this JsonObject jsonObject, string propertyName, JsonNode? propertyValue, bool mutateOriginal = false)`

Sets a property on a JSON object. By default, returns a new object leaving the original unchanged. Set `mutateOriginal` to `true` to modify the original object.

```csharp
var obj = new JsonObject();
var updated = obj.SetProperty("name", JsonValue.Create("John"));
// Returns: New JsonObject with "name" property set, original obj unchanged

var mutated = obj.SetProperty("name", JsonValue.Create("John"), mutateOriginal: true);
// Returns: Same JsonObject instance with "name" property set, original obj modified
```

#### `RemoveProperty(this JsonObject jsonObject, string propertyName, bool mutateOriginal = false)`

Removes a property from a JSON object. By default, returns a new object leaving the original unchanged. Set `mutateOriginal` to `true` to modify the original object.

```csharp
var obj = JsonNode.Parse("""{"name": "John", "age": 30}""").AsObject();
var updated = obj.RemoveProperty("age");
// Returns: New JsonObject without "age" property, original obj unchanged

var mutated = obj.RemoveProperty("age", mutateOriginal: true);
// Returns: Same JsonObject instance without "age" property, original obj modified
```

#### `From(BinaryData? data, JsonSerializerOptions? options = default)`

Converts binary data to a JSON object.

```csharp
var data = BinaryData.FromString("""{"name": "John", "age": 30}""");
var result = JsonObjectModule.From(data);
// Returns: Success(JsonObject)

var arrayData = BinaryData.FromString("""[1, 2, 3]""");
var errorResult = JsonObjectModule.From(arrayData);
// Returns: Error("Deserialization return a null result.")
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

#### `GetElements<T>(this JsonArray jsonArray, Func<JsonNode?, Result<T>> selector, Func<int, Error> errorFromIndex)`

Extracts elements from a JSON array using a selector function, collecting successes or aggregating errors. This is the foundation method that other specialized extraction methods build upon.

```csharp
var array = JsonNode.Parse("""["1", "2", "invalid", "4"]""").AsArray();

// Extract as integers with custom error messages
var result = array.GetElements(
    node => node.AsJsonValue().Bind(v => v.AsInt()),
    index => Error.From($"Element at position {index} is not a valid integer")
);
// Returns: Error with details about which elements failed

var validArray = JsonNode.Parse("""["1", "2", "3"]""").AsArray();
var successResult = validArray.GetElements(
    node => node.AsJsonValue().Bind(v => v.AsInt()),
    index => Error.From($"Invalid at {index}")
);
// Returns: Success(ImmutableArray<int> [1, 2, 3])

// Custom transformations
var stringArray = JsonNode.Parse("""["hello", "world"]""").AsArray();
var upperResult = stringArray.GetElements(
    node => node.AsJsonValue().Bind(v => v.AsString().Map(s => s.ToUpper())),
    index => Error.From($"Not a string at {index}")
);
// Returns: Success(ImmutableArray<string> ["HELLO", "WORLD"])
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
