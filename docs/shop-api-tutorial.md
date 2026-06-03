# Shop Service — Frontend Integration Guide

Base URL (dev): `http://localhost:5104`

- `GET /api/products` and `GET /api/products/{id}` are **public** — no token required.
- All other endpoints require the **Admin** role (`ROLE_Admin`).

## Authentication

For admin-only endpoints, include the JWT bearer token:

```http
Authorization: Bearer <token>
```

> The token is obtained from the Identity service at `http://localhost:5051`.  
> Token format: RS256 JWT with claims `sub` (user UUID), `email`, `role` (Admin | Member | Coach).

---

## Table of Contents

1. [React — Product Management (Admin)](#1-react--product-management)
2. [React — Purchase Processing (Admin)](#2-react--purchase-processing-admin-only)
3. [Flutter — Browse Products (Public)](#3-flutter--browse-products-public)
4. [Error Handling](#4-error-handling)
5. [Image URLs](#5-image-urls)

---

## 1. React — Product Management

### Setup

```tsx
const API = "http://localhost:5104";

function authHeaders(): HeadersInit {
  const token = localStorage.getItem("jwt_token");
  return { Authorization: `Bearer ${token}` };
}
```

---

### 1.1 List All Products (Public)

**User action:** Anyone visits the shop page.

**Endpoint:** `GET /api/products` — **No token required.**

**Response (200):**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Whey Protein",
    "description": "Vanilla flavoured whey protein 2kg",
    "price": 49.99,
    "category": "supplements",
    "imagePath": "/uploads/550e8400-e29b-41d4-a716-446655440000.jpg",
    "stock": 25,
    "isActive": true,
    "createdAt": "2026-06-02T10:30:00Z",
    "updatedAt": null
  }
]
```

**React:**
```tsx
async function fetchProducts() {
  const res = await fetch(`${API}/api/products`);
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // ProductResponse[]
}
```

**Display hint:** Use `imagePath` prefixed with `API` as the `<img>` source:

```tsx
<img src={`${API}${item.imagePath}`} alt={item.name} />
```

**Stock badge component:**
```tsx
function StockBadge({ stock }: { stock: number }) {
  if (stock === 0) return <span className="badge badge-red">Out of stock</span>;
  if (stock <= 5) return <span className="badge badge-amber">Low stock ({stock})</span>;
  return <span className="badge badge-green">In stock ({stock})</span>;
}
```

---

### 1.2 Get Product by ID (Public)

**User action:** User clicks on a product to view details.

**Endpoint:** `GET /api/products/{id}` — **No token required.**

**Response (200):** Same shape as a single list item above.

**Response (404):**
```json
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "detail": "Product not found"
}
```

**React:**
```tsx
async function getProduct(id: string) {
  const res = await fetch(`${API}/api/products/${id}`);
  if (res.status === 404) {
    // Show "Product not found" to user
    return null;
  }
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}
```

---

### 1.3 Create Product (Admin only)

**User action:** Admin clicks "Add Product" → fills in name, price, category, stock → optionally selects an image → clicks "Save".

**Endpoint:** `POST /api/products`

**Request:** `multipart/form-data`

| Field         | Type    | Required | Example            |
|--------------|---------|----------|--------------------|
| `name`        | string  | yes      | "Whey Protein"     |
| `price`       | string* | yes      | "49.99"            |
| `description` | string  | no       | "Vanilla 2kg"      |
| `category`    | string  | no       | "supplements"      |
| `stock`       | string* | no       | "25" (default: 0)  |
| `image`       | file    | no       | .jpg, .png, .webp  |

> \* `price` and `stock` are sent as **strings** in `FormData` and parsed to `BigDecimal` / `Integer` server-side.

**Response (201):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Whey Protein",
  "description": "Vanilla flavoured whey protein 2kg",
  "price": 49.99,
  "category": "supplements",
  "imagePath": "/uploads/550e8400-e29b-41d4-a716-446655440000.jpg",
  "stock": 25,
  "isActive": true,
  "createdAt": "2026-06-02T10:30:00Z",
  "updatedAt": null
}
```

**React (using `fetch` with `FormData`):**
```tsx
async function createProduct(data: {
  name: string;
  price: string;
  description?: string;
  category?: string;
  stock?: string;
  image?: File;
}) {
  const formData = new FormData();
  formData.append("name", data.name);
  formData.append("price", data.price);
  if (data.description) formData.append("description", data.description);
  if (data.category)    formData.append("category", data.category);
  if (data.stock)       formData.append("stock", data.stock);
  if (data.image)       formData.append("image", data.image);

  const res = await fetch(`${API}/api/products`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${localStorage.getItem("jwt_token")}`,
      // Do NOT set Content-Type — browser sets it automatically with boundary
    },
    body: formData,
  });

  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to create product");
  }

  return res.json();
}
```

> ⚠️ **Crucial:** Do NOT set `Content-Type` header manually when sending `FormData`. The browser must set it with the correct multipart boundary.

**File input component:**
```tsx
function ProductForm() {
  const [image, setImage] = useState<File | null>(null);

  return (
    <form onSubmit={handleSubmit}>
      <input name="name" placeholder="Product name" required />
      <input name="price" type="number" step="0.01" placeholder="Price" required />
      <input name="description" placeholder="Description" />
      <select name="category">
        <option value="supplements">Supplements</option>
        <option value="clothing">Clothing</option>
        <option value="accessories">Accessories</option>
        <option value="other">Other</option>
      </select>
      <input name="stock" type="number" min="0" placeholder="Stock" />
      <input type="file" accept="image/*" onChange={e => setImage(e.target.files?.[0] ?? null)} />
      <button type="submit">Save</button>
    </form>
  );
}
```

---

### 1.4 Update Product (Admin only)

**User action:** Admin clicks "Edit" on a product → modifies fields → optionally changes image → clicks "Save".

**Endpoint:** `PUT /api/products/{id}`

**Request:** `multipart/form-data` (all fields optional — only send what changed)

| Field         | Type    | Required | Example         |
|--------------|---------|----------|-----------------|
| `name`        | string  | no       | "Whey Protein V2" |
| `description` | string  | no       | "Updated description" |
| `price`       | string* | no       | "54.99"         |
| `category`    | string  | no       | "supplements"   |
| `stock`       | string* | no       | "30"            |
| `image`       | file    | no       | new image       |

**Response (200):** Updated `ProductResponse` JSON.

**React:**
```tsx
async function updateProduct(id: string, data: {
  name?: string;
  description?: string;
  price?: string;
  category?: string;
  stock?: string;
  image?: File;
}) {
  const formData = new FormData();
  if (data.name)        formData.append("name", data.name);
  if (data.description) formData.append("description", data.description);
  if (data.price)       formData.append("price", data.price);
  if (data.category)    formData.append("category", data.category);
  if (data.stock)       formData.append("stock", data.stock);
  if (data.image)       formData.append("image", data.image);

  const res = await fetch(`${API}/api/products/${id}`, {
    method: "PUT",
    headers: { Authorization: `Bearer ${localStorage.getItem("jwt_token")}` },
    body: formData,
  });

  if (!res.ok) throw new Error((await res.json()).detail);
  return res.json();
}
```

---

### 1.5 Delete Product — Soft Delete (Admin only)

**User action:** Admin clicks "Delete" → confirmation dialog → confirms.

**Endpoint:** `DELETE /api/products/{id}`

**Response (204):** No content. The product is marked as inactive (soft delete) — it will no longer appear in the public product list but remains in the database.

**React:**
```tsx
async function deleteProduct(id: string) {
  const confirmed = window.confirm(
    "Are you sure you want to delete this product?"
  );
  if (!confirmed) return;

  const res = await fetch(`${API}/api/products/${id}`, {
    method: "DELETE",
    headers: authHeaders(),
  });

  if (res.status === 204) {
    // Success — remove from UI list
    return;
  }
  if (res.status === 404) {
    alert("Product not found — it may have been already deleted.");
    return;
  }
  throw new Error(await res.text());
}
```

---

## 2. React — Purchase Processing (Admin only)

### 2.1 List All Purchases

**User action:** Admin opens the purchase history page.

**Endpoint:** `GET /api/purchases`

**Response (200):**
```json
[
  {
    "id": "660e8400-e29b-41d4-a716-446655440001",
    "memberId": null,
    "productId": "550e8400-e29b-41d4-a716-446655440000",
    "quantity": 2,
    "purchasedAt": "2026-06-02T14:30:00Z"
  }
]
```

**React:**
```tsx
async function fetchPurchases() {
  const res = await fetch(`${API}/api/purchases`, {
    headers: authHeaders(),
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json(); // PurchaseResponse[]
}
```

---

### 2.2 Process a Purchase

**User action:** Admin scans a product barcode → optionally selects a member → enters quantity → clicks "Purchase".  
Product stock is automatically deducted from inventory.

**Endpoint:** `POST /api/purchases`

**Request body (JSON):**
```json
{
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "memberId": "770e8400-e29b-41d4-a716-446655440002",
  "quantity": 2
}
```

| Field       | Type   | Required | Default | Description |
|------------|--------|----------|---------|-------------|
| `productId` | string | yes      | —       | UUID of the product |
| `memberId`  | string | no       | `null`  | UUID of the member (omit or null for walk-in sales) |
| `quantity`  | number | no       | `1`     | How many items purchased |

**Response (201):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440001",
  "memberId": "770e8400-e29b-41d4-a716-446655440002",
  "productId": "550e8400-e29b-41d4-a716-446655440000",
  "quantity": 2,
  "purchasedAt": "2026-06-02T14:30:00Z"
}
```

**Response (400 — Out of Stock):**
```json
{
  "type": "about:blank",
  "title": "Bad Request",
  "status": 400,
  "detail": "Out of stock. Available: 3, requested: 5"
}
```

**React:**
```tsx
async function processPurchase(data: {
  productId: string;
  memberId?: string;
  quantity?: number;
}) {
  const body: Record<string, unknown> = {
    productId: data.productId,
  };
  if (data.memberId) body.memberId = data.memberId;
  if (data.quantity != null) body.quantity = data.quantity;

  const res = await fetch(`${API}/api/purchases`, {
    method: "POST",
    headers: {
      ...authHeaders(),
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    const error = await res.json();
    throw new Error(error.detail || "Failed to process purchase");
  }

  return res.json();
}
```

**Purchase form example:**
```tsx
function PurchaseForm({ products }: { products: ProductResponse[] }) {
  const [selectedProduct, setSelectedProduct] = useState("");
  const [memberId, setMemberId] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [message, setMessage] = useState("");

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    try {
      const result = await processPurchase({
        productId: selectedProduct,
        memberId: memberId || undefined,
        quantity,
      });
      setMessage(`Purchase recorded! ${result.quantity}x items`);
    } catch (err: any) {
      setMessage(`Error: ${err.message}`);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <select
        value={selectedProduct}
        onChange={e => setSelectedProduct(e.target.value)}
        required
      >
        <option value="">Select a product...</option>
        {products.map(p => (
          <option key={p.id} value={p.id}>
            {p.name} — {p.stock} in stock (${p.price})
          </option>
        ))}
      </select>
      <input
        placeholder="Member ID (optional)"
        value={memberId}
        onChange={e => setMemberId(e.target.value)}
      />
      <input
        type="number"
        min="1"
        value={quantity}
        onChange={e => setQuantity(Number(e.target.value))}
        required
      />
      <button type="submit">Purchase</button>
      {message && <p>{message}</p>}
    </form>
  );
}
```

---

## 3. Flutter — Browse Products (Public)

**User action:** A member opens the shop tab to browse products.

> Currently the shop is admin-processed only. Member purchases via mobile app are planned as a **future extension**. For now, the Flutter app can display the product catalogue.

### Setup

```dart
const String apiBase = "http://localhost:5104";
```

> For public `GET` endpoints, **no token is required**. No auth setup needed for browsing.

---

### Browse All Products

**Endpoint:** `GET /api/products`

**Response (200):** See [1.1 List All Products](#11-list-all-products-public) for the JSON shape.

**Flutter:**
```dart
import 'dart:convert';
import 'package:http/http.dart' as http;

class ProductService {
  final String baseUrl;

  ProductService({required this.baseUrl});

  /// Fetch all active products from the shop.
  Future<List<ProductResponse>> fetchProducts() async {
    final uri = Uri.parse('$baseUrl/api/products');
    final response = await http.get(uri);

    if (response.statusCode == 200) {
      final List<dynamic> body = jsonDecode(response.body);
      return body.map((e) => ProductResponse.fromJson(e)).toList();
    }

    throw Exception("Failed to load products");
  }

  /// Fetch a single product by ID.
  Future<ProductResponse> getProduct(String id) async {
    final uri = Uri.parse('$baseUrl/api/products/$id');
    final response = await http.get(uri);

    if (response.statusCode == 200) {
      return ProductResponse.fromJson(jsonDecode(response.body));
    }
    if (response.statusCode == 404) {
      throw Exception("Product not found");
    }

    throw Exception("Failed to load product");
  }
}
```

**Data model:**
```dart
class ProductResponse {
  final String id;
  final String name;
  final String? description;
  final double price;
  final String? category;
  final String? imagePath;
  final int stock;
  final bool isActive;
  final DateTime createdAt;
  final DateTime? updatedAt;

  ProductResponse({
    required this.id,
    required this.name,
    this.description,
    required this.price,
    this.category,
    this.imagePath,
    required this.stock,
    required this.isActive,
    required this.createdAt,
    this.updatedAt,
  });

  factory ProductResponse.fromJson(Map<String, dynamic> json) {
    return ProductResponse(
      id: json['id'],
      name: json['name'],
      description: json['description'],
      price: (json['price'] as num).toDouble(),
      category: json['category'],
      imagePath: json['imagePath'],
      stock: json['stock'],
      isActive: json['isActive'],
      createdAt: DateTime.parse(json['createdAt']),
      updatedAt:
          json['updatedAt'] != null ? DateTime.parse(json['updatedAt']) : null,
    );
  }
}
```

**Screen example:**
```dart
class ShopScreen extends StatefulWidget {
  @override
  State<ShopScreen> createState() => _ShopScreenState();
}

class _ShopScreenState extends State<ShopScreen> {
  final _service = ProductService(baseUrl: "http://localhost:5104");
  late Future<List<ProductResponse>> _productsFuture;

  @override
  void initState() {
    super.initState();
    _productsFuture = _service.fetchProducts();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Shop")),
      body: FutureBuilder<List<ProductResponse>>(
        future: _productsFuture,
        builder: (context, snapshot) {
          if (snapshot.hasError) {
            return Center(child: Text("Error: ${snapshot.error}"));
          }
          if (!snapshot.hasData) {
            return const Center(child: CircularProgressIndicator());
          }

          final products = snapshot.data!;
          return ListView.builder(
            itemCount: products.length,
            itemBuilder: (context, index) {
              final p = products[index];
              return Card(
                margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                child: ListTile(
                  leading: p.imagePath != null
                      ? ClipRRect(
                          borderRadius: BorderRadius.circular(8),
                          child: Image.network(
                            'http://localhost:5104${p.imagePath}',
                            width: 60,
                            height: 60,
                            fit: BoxFit.cover,
                          ),
                        )
                      : const Icon(Icons.shopping_bag, size: 40),
                  title: Text(p.name),
                  subtitle: Text(
                    p.category ?? "",
                    style: const TextStyle(color: Colors.grey),
                  ),
                  trailing: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: [
                      Text(
                        '\$${p.price.toStringAsFixed(2)}',
                        style: const TextStyle(
                          fontWeight: FontWeight.bold,
                          fontSize: 16,
                        ),
                      ),
                      Text(
                        p.stock > 0 ? "${p.stock} in stock" : "Out of stock",
                        style: TextStyle(
                          color: p.stock > 0 ? Colors.green : Colors.red,
                          fontSize: 12,
                        ),
                      ),
                    ],
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
```

---

## 4. Error Handling

All error responses follow the RFC 9457 Problem Details format:

```json
{
  "type": "about:blank",
  "title": "Not Found",
  "status": 404,
  "detail": "Product not found"
}
```

| HTTP Status | Meaning | Common Causes |
|------------|---------|---------------|
| **200** | Success | GET, PUT |
| **201** | Created | POST (product, purchase) |
| **204** | No Content | DELETE |
| **400** | Bad Request | Out of stock, missing required field, invalid UUID format |
| **401** | Unauthorized | Missing or expired JWT |
| **403** | Forbidden | Valid JWT but missing Admin role |
| **404** | Not Found | Product ID doesn't exist or is soft-deleted |
| **500** | Server Error | Backend exception |

**React generic error handler:**
```tsx
async function handleApiResponse(res: Response): Promise<any> {
  if (res.ok) {
    if (res.status === 204) return null;
    return res.json();
  }
  const body = await res.json().catch(() => ({ detail: res.statusText }));
  throw new ApiError(body.status || res.status, body.detail || "Unknown error");
}

class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}
```

**Flutter error handling:**
```dart
class ShopException implements Exception {
  final String message;
  ShopException(this.message);
  @override
  String toString() => message;
}

// Wrap API calls:
Future<T> handleShopResponse<T>(http.Response response, T Function(Map<String, dynamic>) parser) async {
  if (response.statusCode >= 200 && response.statusCode < 300) {
    return parser(jsonDecode(response.body));
  }
  final error = jsonDecode(response.body);
  throw ShopException(error['detail'] ?? 'Unknown error');
}
```

---

## 5. Image URLs

The `imagePath` field contains a **relative path** like `/uploads/uuid.jpg`.

To display the image, prepend the API base URL:

**React:**
```tsx
<img src={`http://localhost:5104${product.imagePath}`} alt={product.name} />
```

**Flutter:**
```dart
Image.network('http://localhost:5104${product.imagePath}')
```

The images are served as **static files** directly from the Java service's file system — no additional endpoint needed.

---

## Endpoint Reference Card

| Method | Path | Auth | Body | Returns |
|--------|------|------|------|---------|
| `GET` | `/api/products` | **Public** | — | `ProductResponse[]` |
| `GET` | `/api/products/{id}` | **Public** | — | `ProductResponse` |
| `POST` | `/api/products` | Admin | multipart form | `ProductResponse` (201) |
| `PUT` | `/api/products/{id}` | Admin | multipart form | `ProductResponse` |
| `DELETE` | `/api/products/{id}` | Admin | — | (204) |
| `GET` | `/api/purchases` | Admin | — | `PurchaseResponse[]` |
| `POST` | `/api/purchases` | Admin | `{ "productId": "...", "memberId": "...", "quantity": 1 }` | `PurchaseResponse` (201) |

---

### Quick Reference

**Product categories:** `supplements` | `clothing` | `accessories` | `other`

**Roles:** `Admin` | `Member` | `Coach` (from JWT role claim)

**Base URLs:**
- Shop service: `http://localhost:5104`
- Identity service: `http://localhost:5051`
