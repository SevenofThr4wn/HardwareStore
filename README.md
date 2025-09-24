
# 🚀 Keycloak Server Setup

---

## 📦 1. Install Docker
- [Download Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Mac) or install via package manager on Linux.  
- Verify installation:  
  ```bash
  docker --version
---

## 🛠️ 2. Pull the Keycloak Image

```bash
docker pull quay.io/keycloak/keycloak:latest
```

---

## ▶️ 3. Run Keycloak Container

Start Keycloak in **development mode** with an embedded database:

```bash
docker run -d \
  --name keycloak \
  -p 8080:8080 \
  -e KEYCLOAK_ADMIN=admin \
  -e KEYCLOAK_ADMIN_PASSWORD=admin \
  quay.io/keycloak/keycloak:latest \
  start-dev
```

* `KEYCLOAK_ADMIN` → admin username
* `KEYCLOAK_ADMIN_PASSWORD` → admin password
* `-p 8080:8080` → access Keycloak at [http://localhost:8080](http://localhost:8080)

View logs:

```bash
docker logs -f keycloak
```

---

## 🔑 4. Access the Admin Console

* Open: [http://localhost:8080](http://localhost:8080)
* Click **Administration Console**
* Login with `admin / admin` (or the credentials you set).

---

## 🌍 5. Create a Realm

1. In the left sidebar → select **Realm selector** → **Create Realm**
2. Name it: `HardwareStore`
3. Save

---

## 🖥️ 6. Create a Client

1. Go to **Clients** → **Create client**
2. Set values:

   * Client ID: `hardwarestore-client`
   * Name: `HardwareStore Client`
   * Client type: **OpenID Connect**
3. Configure:

   * Root URL: `https://localhost:7267`
   * Valid redirect URIs: `https://localhost:7267/signin-oidc`
   * Web origins: add your frontend URL
4. Save

---

## 👥 7. Create Roles

1. Go to **Realm Roles** → **Create Role**
2. Add roles:

   * `Manager`
   * `Admin`
   * `Staff`
   * `User`

---

## 👤 8. Create a User

1. Navigate to **Users** → **Add user**
2. Enter username/email → Save
3. Go to **Credentials** → set password (disable **Temporary**)
4. Assign roles:

   * Go to **Role Mappings**
   * Add `Admin` / `Manager` / `Staff`

---

## 📜 9. Add Role Mapper to Token

1. Go to **Clients** → select `hardwarestore-client`
2. Navigate to **Client scopes**
3. Add roles mapper:

   * Name: `roles`
   * Mapper type: **User Realm Role**
   * Token Claim Name: `roles`
   * Add to ID Token: ✅
   * Add to Access Token: ✅
   * Add to UserInfo: ✅
4. Add realm_access Mapper:
   * Name: `realm roles`
   * Mapper type: **User Realm Role**
   * Multivalued: ✅
   * Token Claim Name: `realm_access`
   * Claim JSON Type: `JSON`
   * Add to ID Token: ✅
   * Add to Access Token: ✅
   * Add to UserInfo: ✅
5. Add resource_access
   * Name: client roles
   * Mapper Type: **User Client Role**
   * Client ID: hardware-store-client
   * Multivalued: ✅
   * Token Claim Name: ✅
   * Add to ID Token: ✅
   * Add to Access Token: ✅
   * Add to UserInfo: ✅
  
---

## 📌 Notes

* This setup uses the **embedded database** (--> not production-ready <--).
* To persist data, mount a Docker volume for Keycloak.
* Always create strong admin passwords in production.

---
