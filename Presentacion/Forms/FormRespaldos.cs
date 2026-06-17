using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Servicios;

namespace Presentacion.Forms
{
    /// <summary>
    /// Respaldos y restauración de la base de datos (solo administrador). Form-hijo del shell,
    /// construido en código. Con SQL Server muestra un aviso (los respaldos van en el servidor).
    /// </summary>
    public class FormRespaldos : Form
    {
        private readonly RespaldoService respaldoService = new RespaldoService();

        private Panel  pnlForm;
        private Label  lblInfo;
        private Button btnRespaldar, btnRestaurar, btnRestaurarArchivo, btnAbrirCarpeta;
        private DataGridView dgv;

        public FormRespaldos()
        {
            this.Text = "Respaldos";
            ConstruirUI();
            this.Load += FormRespaldos_Load;
        }

        private void ConstruirUI()
        {
            dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.Columns.Add("colRuta", "Ruta");
            dgv.Columns.Add("colFecha", "Fecha");
            dgv.Columns.Add("colNombre", "Archivo");
            dgv.Columns.Add("colTamano", "Tamaño");
            dgv.Columns.Add("colUbicacion", "Ubicación");
            dgv.Columns["colRuta"].Visible = false;
            dgv.Columns["colTamano"].FillWeight = 50;
            dgv.Columns["colUbicacion"].FillWeight = 50;
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) RestaurarSeleccionado(); };

            pnlForm = new Panel { Dock = DockStyle.Top, Height = 156 };

            lblInfo = new Label { AutoSize = false, Location = new Point(20, 14), Size = new Size(900, 64) };

            btnRespaldar        = new Button { Text = "Respaldar ahora",        Location = new Point(20, 92),  Size = new Size(170, 42) };
            btnRestaurar        = new Button { Text = "Restaurar seleccionado",  Location = new Point(200, 92), Size = new Size(200, 42) };
            btnRestaurarArchivo = new Button { Text = "Restaurar desde archivo…", Location = new Point(410, 92), Size = new Size(210, 42) };
            btnAbrirCarpeta     = new Button { Text = "Abrir carpeta",           Location = new Point(630, 92), Size = new Size(150, 42) };

            btnRespaldar.Click        += (s, e) => RespaldarAhora();
            btnRestaurar.Click        += (s, e) => RestaurarSeleccionado();
            btnRestaurarArchivo.Click += (s, e) => RestaurarDesdeArchivo();
            btnAbrirCarpeta.Click     += (s, e) => AbrirCarpeta();

            pnlForm.Controls.AddRange(new Control[] {
                lblInfo, btnRespaldar, btnRestaurar, btnRestaurarArchivo, btnAbrirCarpeta });

            this.Controls.Add(dgv);
            this.Controls.Add(pnlForm);
        }

        private void FormRespaldos_Load(object sender, EventArgs e)
        {
            AplicarEstilos();
            EstiloPos.AplicarGrid(dgv);

            if (!respaldoService.SoportaArchivo)
            {
                // SQL Server: el respaldo lo gestiona el servidor.
                lblInfo.Text = "Con SQL Server, los respaldos se gestionan en el servidor de base de datos " +
                               "(plan de mantenimiento / BACKUP DATABASE). Consulta la guía de despliegue. " +
                               "Esta pantalla opera respaldos por archivo solo cuando el motor es SQLite.";
                foreach (var b in new[] { btnRespaldar, btnRestaurar, btnRestaurarArchivo, btnAbrirCarpeta })
                    b.Enabled = false;
                return;
            }

            Cargar();
        }

        private void AplicarEstilos()
        {
            this.BackColor    = EstiloPos.Fondo;
            pnlForm.BackColor = EstiloPos.Surface;
            lblInfo.Font      = EstiloPos.FontSmall;
            lblInfo.ForeColor = EstiloPos.Ink2;

            EstiloPos.AplicarBotonPrimario(btnRespaldar);
            EstiloPos.AplicarBotonSecundario(btnRestaurar);
            EstiloPos.AplicarBotonSecundario(btnRestaurarArchivo);
            EstiloPos.AplicarBotonSecundario(btnAbrirCarpeta);
        }

        private void Cargar()
        {
            string externa = string.IsNullOrWhiteSpace(respaldoService.CarpetaExterna)
                ? "no configurada (define «CarpetaRespaldoExterno» en App.config para copiar fuera del disco)"
                : respaldoService.CarpetaExterna;
            lblInfo.Text = "Carpeta local:  " + respaldoService.CarpetaLocal +
                           "\nCarpeta externa:  " + externa +
                           "\nSe crea un respaldo automático al iniciar (uno por día). Doble clic en un respaldo para restaurarlo.";

            dgv.SuspendLayout();
            dgv.Rows.Clear();
            foreach (var r in respaldoService.Obtener())
                dgv.Rows.Add(r.Ruta, r.Fecha.ToString("dd/MM/yyyy HH:mm"), r.Nombre, r.TamanoLegible, r.Ubicacion);
            dgv.ResumeLayout();
        }

        private void RespaldarAhora()
        {
            try
            {
                string ruta = respaldoService.RespaldarAhora();
                string extra = string.IsNullOrWhiteSpace(respaldoService.CarpetaExterna)
                    ? "" : "\nTambién se copió a la carpeta externa.";
                Aviso.Exito(this, "Respaldo creado: " + System.IO.Path.GetFileName(ruta) + extra, "Respaldo listo");
                Cargar();
            }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo respaldar"); }
        }

        private void RestaurarSeleccionado()
        {
            if (dgv.CurrentRow == null) { Aviso.Info(this, "Selecciona un respaldo de la lista.", "Restaurar"); return; }
            RestaurarDesde(dgv.CurrentRow.Cells["colRuta"].Value?.ToString(),
                           dgv.CurrentRow.Cells["colNombre"].Value?.ToString());
        }

        private void RestaurarDesdeArchivo()
        {
            using (var ofd = new OpenFileDialog
            {
                Title = "Selecciona un archivo de respaldo",
                Filter = "Respaldo de base de datos (*.db)|*.db|Todos los archivos (*.*)|*.*"
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                    RestaurarDesde(ofd.FileName, System.IO.Path.GetFileName(ofd.FileName));
            }
        }

        private void RestaurarDesde(string ruta, string nombre)
        {
            if (string.IsNullOrWhiteSpace(ruta)) return;
            if (!Aviso.Confirmar(this,
                    "Se REEMPLAZARÁN todos los datos actuales por los del respaldo «" + nombre + "».\n" +
                    "Se guardará una copia del estado actual (.previo) por si necesitas volver.\n" +
                    "La aplicación se reiniciará para aplicar la restauración.",
                    "¿Restaurar la base de datos?", "Restaurar", TipoAviso.Error))
                return;
            try
            {
                respaldoService.Restaurar(ruta);
                Aviso.Exito(this, "La base de datos se restauró. La aplicación se reiniciará ahora.", "Restauración completa");
                Application.Restart();
            }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo restaurar"); }
        }

        private void AbrirCarpeta()
        {
            try
            {
                string carpeta = respaldoService.CarpetaLocal;
                System.IO.Directory.CreateDirectory(carpeta);
                Process.Start("explorer.exe", "\"" + carpeta + "\"");
            }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo abrir la carpeta"); }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F5 && respaldoService.SoportaArchivo) { Cargar(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
