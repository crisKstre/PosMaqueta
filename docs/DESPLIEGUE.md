# Guía de despliegue

El sistema funciona con **dos motores de base de datos**, según la necesidad del negocio:

| Escenario | Motor | Cuándo |
|---|---|---|
| **Una sola caja** | **SQLite** (por defecto) | Un único equipo. Cero instalación de servidor. |
| **Varias cajas** | **SQL Server** (Express) | 2+ cajas que comparten el mismo inventario y ventas. |

El motor se elige en el archivo **`App.config`** de cada caja (`ProveedorBD`), sin recompilar.

---

## A. Una sola caja (SQLite) — por defecto

1. Compilar/publicar el proyecto **Presentacion** (`dotnet build -c Release` o el perfil de publicación
   en `Presentacion/Properties/PublishProfiles/`).
2. Copiar la carpeta publicada al equipo.
3. Ejecutar `Presentacion.exe`.

Al primer arranque se crea `pos.db` junto al ejecutable, con tablas, índices, el administrador
por defecto y categorías. No hay que instalar nada más. Respaldos automáticos en `Backups/`.

Para mayor seguridad, configura una **carpeta de respaldo externa** (red/USB/nube) en `App.config`
(`CarpetaRespaldoExterno`): cada respaldo se copia también ahí, **fuera del disco**. Desde la app,
el módulo **Respaldos** (admin) permite respaldar al instante y **restaurar** una copia.

```xml
<add key="CarpetaRespaldoExterno" value="\\SERVIDOR\Respaldos\POS" />
```

`App.config` (valor por defecto):

```xml
<add key="ProveedorBD" value="Sqlite" />
```

---

## B. Varias cajas (SQL Server Express) — inventario y ventas compartidos

### B.1 En el PC servidor (una vez)

1. **Instalar SQL Server Express** (gratis). Durante la instalación, habilitar **autenticación de
   Windows** (o mixta si se usará usuario/clave de SQL).
2. **Habilitar TCP/IP**: SQL Server Configuration Manager → *Protocolos* → habilitar **TCP/IP** →
   reiniciar el servicio.
3. **Abrir el firewall**: permitir el puerto del servicio (por defecto **1433**, o el de la instancia).
4. Anotar el **nombre del servidor/instancia** (p. ej. `SERVIDOR\SQLEXPRESS`).
5. No hace falta crear la base a mano: la app crea **`PosMaqueta`** (tablas, índices y seed) en el
   primer arranque si no existe.

> El PC servidor debe estar **encendido** mientras opere cualquier caja. **No hay modo offline**:
> si se cae el servidor o la red, las cajas no pueden vender.

### B.2 En cada caja

1. Copiar la app (igual que en SQLite).
2. Editar **`Presentacion.exe.config`** (el `App.config` publicado) y apuntar al servidor:

```xml
<appSettings>
  <add key="ProveedorBD" value="SqlServer" />
  <add key="CadenaConexion"
       value="Server=SERVIDOR\SQLEXPRESS;Database=PosMaqueta;Integrated Security=true;TrustServerCertificate=true;" />
</appSettings>
```

- Con **autenticación de Windows**: `Integrated Security=true` (la cuenta de Windows de la caja
  debe tener acceso a SQL Server).
- Con **usuario/clave de SQL**: `User ID=pos;Password=...;` en vez de `Integrated Security`.

3. Ejecutar. La **primera** caja que arranque crea la base `PosMaqueta`; las demás la encuentran ya
   creada. Todas comparten inventario, ventas, caja y usuarios.

### B.3 Respaldos (SQL Server)

La copia automática a `Backups/` **solo aplica a SQLite**. Con SQL Server, configurar el respaldo
en el **servidor**:

- Un **plan de mantenimiento** o un job de SQL Agent (en Express, usar el Programador de tareas de
  Windows con `sqlcmd`), p. ej.:

```sql
BACKUP DATABASE PosMaqueta TO DISK = 'D:\Backups\PosMaqueta.bak' WITH INIT, COMPRESSION;
```

- Guardar los respaldos **fuera del disco del servidor** (otra unidad / red / nube) para
  sobrevivir a una falla de disco.

---

## Requisitos

- **Todas:** Windows 10/11, .NET Framework 4.7.2, permiso de escritura en la carpeta del ejecutable
  (para `pos.db`, `Backups/` y `Logs/`).
- **Multi-caja:** SQL Server Express en el servidor + red local (LAN) entre las cajas y el servidor.

## Verificación rápida

- Iniciar sesión con `admin` / `admin123` (cambiar la contraseña del admin antes de producción).
- Registrar una venta y confirmar que el stock baja.
- En multi-caja: vender en una caja y verificar que el stock se actualiza en otra (mismo servidor).

---

Ver también: [ARQUITECTURA.md](ARQUITECTURA.md) · [MODELO-DATOS.md](MODELO-DATOS.md) · [MANUAL-USUARIO.md](MANUAL-USUARIO.md)
