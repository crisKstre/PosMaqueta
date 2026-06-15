# Sistema POS

Punto de venta (POS) de escritorio para **minimarket / almacén**, desarrollado en
**C# · WinForms · .NET Framework 4.7.2** con base de datos **SQLite local** (sin servidor).

## 🎯 Objetivo del proyecto

Un POS **de uso interno** —operado por empleados y el administrador del negocio, no
por el cliente— pensado para ser **simple, rápido y robusto** en el día a día de una
caja. Diseñado para Chile (pesos chilenos, IVA 19 %).

Principios:

- **Modular y mantenible:** arquitectura en capas; cada módulo se arma sin tocar el resto.
- **Conectable a hardware:** compatible con escáner de código de barras (USB/HID) de
  inmediato; preparado para sumar cajón de dinero a futuro.
- **La boleta la emite una máquina aparte** (terminal de pago): el sistema registra y
  controla la venta internamente, no genera el comprobante tributario.
- **Cero dependencia de internet:** todo funciona local.

---

## ✨ Funcionalidades

### Núcleo
- Login con **roles** (Administrador / Cajero) y contraseña con hash SHA256.
- Base de datos SQLite **autocreada** al primer arranque, con **migraciones** incrementales.
- **Respaldo automático** diario de la base de datos (carpeta `Backups/`, con rotación).
- Shell con **sidebar estilo POS** (íconos, ítem activo resaltado).
- Control de acceso por rol y diálogos de aviso unificados (éxito / error / confirmación).
- Auditoría: registro de movimientos (altas, bajas, ventas, anulaciones…).

### 🏠 Inicio (Dashboard)
- Métricas del día: ventas, total vendido, productos bajo stock y estado de la caja.

### 🛒 Ventas
- Lector de **código de barras** y búsqueda por nombre con sugerencias.
- Carrito editable (− / + por ítem) y **varias ventas en paralelo** (pestañas navegables).
- **Descuento** sobre el total y **desglose de IVA** (neto + IVA 19 %).
- Cobro con 3 medios de pago; en efectivo, **cálculo de vuelto**.
- Registro de ventas con **detalle por código** (doble clic para ver los ítems).
- Descuento de stock y registro de la venta en una transacción.

### 📦 Productos / Inventario
- Alta, edición, baja y desactivación de productos; gestión de categorías.
- Búsqueda en vivo, control de stock (entrada / salida) y **alerta de stock mínimo**.
- Soporte por unidad o por peso (kg).
- **Solo lectura para empleados** (las modificaciones quedan para el administrador).

### 💵 Caja
- Apertura de turno con monto inicial (solo admin).
- Resumen del turno y **cierre con arqueo** (efectivo esperado vs. contado); un faltante
  requiere autorización de administrador.
- **Histórico de cajas** con filtros por fecha y diferencias de arqueo.

### 📊 Reportes (solo admin)
- Resumen por período: ventas, total, ticket promedio, IVA y desglose por medio de pago.
- Ranking de **productos más vendidos** y listado de ventas con **detalle por código**.
- **Anulación de ventas** (devolución): revierte el stock y queda auditada.

---

## 🏗️ Arquitectura (capas)

```
Presentacion   WinForms: login, shell con sidebar, formularios de módulos, EstiloPos
     │
Dominio        Servicios (lógica de negocio) + NotificadorCambios (observer)
     │
AccesoData     DAOs con SQL, conexión SQLite, inicializador/migraciones, respaldo
     │
Entidades      Modelos: Usuario, Producto, Venta, DetalleVenta, Caja, etc.
```

- **`EstiloPos`** es la única fuente de verdad para colores, fuentes y tamaños.
- Patrones aplicados: capas, DAO, Observer, Service Layer, y un *facade* de diálogos (`Aviso`).

---

## 💻 Requisitos

**Software**
- Windows 7 SP1 o superior (recomendado Windows 10 / 11).
- .NET Framework **4.7.2** (preinstalado desde Windows 10 1803).
- SQLite va embebido vía NuGet `Microsoft.Data.Sqlite` (se restaura al compilar) — **no hay servidor que instalar**.

**Hardware (mínimo)**
- CPU 1 GHz · 2 GB RAM · ~150 MB de disco · pantalla 1366×768 (ideal 1920×1080).

**Periféricos**
- Escáner de código de barras USB (HID): opcional, plug & play.

> Funciona en un solo equipo (BD local) y sin conexión a internet. Requiere permiso de
> escritura en la carpeta del ejecutable (para `pos.db` y `Backups/`).

---

## ▶️ Cómo arrancar

1. Abre `PosMaqueta.sln` en Visual Studio 2022 (o 2019).
2. Deja que restaure los paquetes NuGet (automático).
3. Proyecto de inicio: **Presentacion**.
4. Ejecuta (F5).

Al primer arranque se crea `pos.db` junto al ejecutable, con las tablas, un administrador
por defecto y categorías de ejemplo.

### Usuarios de prueba

| Usuario     | Contraseña     | Rol           |
|-------------|----------------|---------------|
| `admin`     | `admin123`     | Administrador |
| `empleado`  | `empleado123`  | Cajero        |

---

## 🗺️ Roadmap

- Clientes y **fiado** (cuenta corriente por cliente).
- **Movimientos de caja** (ingresos / egresos de efectivo en el turno).
- Atajos de teclado para operación rápida sin mouse.
- Descuento por porcentaje y por ítem.
- Exportación de reportes (Excel / PDF) y gráficos.
- Apertura de cajón de dinero (ESC/POS) e integración con terminal de pago.

---

## 📝 Notas

- Las contraseñas se guardan con hash SHA256; para producción conviene migrar a PBKDF2 con salt.
- Las boletas/DTE las emite la máquina de pago; este sistema registra la venta internamente.
- La personalización por negocio (nombre, formato, etc.) se hace a nivel de instalación/código.
