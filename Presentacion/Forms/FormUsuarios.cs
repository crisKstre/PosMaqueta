using System;
using System.Drawing;
using System.Windows.Forms;
using Dominio.Eventos;
using Dominio.Servicios;
using Entidades;

namespace Presentacion.Forms
{
    /// <summary>
    /// Gestión de usuarios (solo administrador): alta, edición de perfil, activar/desactivar
    /// y reseteo de contraseña. Construido en código (como FormGestionCategorias) para no
    /// depender del diseñador. Se abre como form-hijo del shell.
    /// </summary>
    public class FormUsuarios : Form
    {
        private readonly UsuarioService usuarioService = new UsuarioService();
        private int idEnEdicion = 0;

        private Panel    pnlForm;
        private Label    lblModo, lblNombre, lblLogin, lblRol, lblPass, lblPassHint, lblError;
        private TextBox  txtNombre, txtLogin, txtPass;
        private ComboBox cmbRol;
        private CheckBox chkActivo;
        private Button   btnGuardar, btnCancelar, btnEstado, btnReset;
        private DataGridView dgv;

        public FormUsuarios()
        {
            this.Text = "Usuarios";
            ConstruirUI();
            this.Load += FormUsuarios_Load;
        }

        private void ConstruirUI()
        {
            dgv = new DataGridView { Dock = DockStyle.Fill };
            dgv.Columns.Add("colId", "Id");
            dgv.Columns.Add("colNombre", "Nombre");
            dgv.Columns.Add("colLogin", "Usuario");
            dgv.Columns.Add("colRol", "Rol");
            dgv.Columns.Add("colEstado", "Estado");
            dgv.Columns.Add("colCambio", "Cambio pendiente");
            dgv.Columns["colId"].Visible = false;
            dgv.Columns["colRol"].FillWeight = 60;
            dgv.Columns["colEstado"].FillWeight = 60;
            dgv.Columns["colCambio"].FillWeight = 80;
            dgv.CellDoubleClick += dgv_CellDoubleClick;
            dgv.SelectionChanged += (s, e) => ActualizarBotonEstado();

            pnlForm = new Panel { Dock = DockStyle.Top, Height = 262 };

            lblModo = new Label { Text = "Nuevo usuario", AutoSize = true, Location = new Point(20, 12) };

            lblNombre = new Label { Text = "NOMBRE", AutoSize = true, Location = new Point(20, 48) };
            txtNombre = new TextBox { Location = new Point(20, 70), Size = new Size(240, 38) };

            lblLogin = new Label { Text = "USUARIO (LOGIN)", AutoSize = true, Location = new Point(280, 48) };
            txtLogin = new TextBox { Location = new Point(280, 70), Size = new Size(200, 38) };

            lblRol = new Label { Text = "ROL", AutoSize = true, Location = new Point(500, 48) };
            cmbRol = new ComboBox { Location = new Point(500, 70), Size = new Size(150, 38) };
            cmbRol.Items.AddRange(RolUsuario.Todos);

            lblPass = new Label { Text = "CONTRASEÑA", AutoSize = true, Location = new Point(20, 120) };
            txtPass = new TextBox { Location = new Point(20, 142), Size = new Size(240, 38), UseSystemPasswordChar = true };
            lblPassHint = new Label { Text = "Usa «Resetear contraseña» para cambiarla.",
                AutoSize = true, Location = new Point(20, 150), Visible = false };

            chkActivo = new CheckBox { Text = "Activo", AutoSize = true, Location = new Point(300, 146), Checked = true };

            lblError = new Label { AutoSize = false, Size = new Size(820, 22), Location = new Point(20, 188), Visible = false };

            btnGuardar  = new Button { Text = "Guardar",  Location = new Point(20, 212),  Size = new Size(150, 42) };
            btnCancelar = new Button { Text = "Cancelar", Location = new Point(180, 212), Size = new Size(120, 42), Visible = false };
            btnEstado   = new Button { Text = "Desactivar", Location = new Point(470, 212), Size = new Size(160, 42) };
            btnReset    = new Button { Text = "Resetear contraseña", Location = new Point(642, 212), Size = new Size(200, 42) };

            btnGuardar.Click  += btnGuardar_Click;
            btnCancelar.Click += (s, e) => LimpiarFormulario();
            btnEstado.Click   += btnEstado_Click;
            btnReset.Click    += btnReset_Click;

            pnlForm.Controls.AddRange(new Control[] {
                lblModo, lblNombre, txtNombre, lblLogin, txtLogin, lblRol, cmbRol,
                lblPass, txtPass, lblPassHint, chkActivo, lblError,
                btnGuardar, btnCancelar, btnEstado, btnReset });

            this.Controls.Add(dgv);
            this.Controls.Add(pnlForm);
        }

        private void FormUsuarios_Load(object sender, EventArgs e)
        {
            AplicarEstilos();
            EstiloPos.AplicarGrid(dgv);
            cmbRol.SelectedIndex = cmbRol.Items.Count > 0 ? 0 : -1;

            NotificadorCambios.Cambio += OnCambioDatos;
            this.Disposed += (s, ev) => NotificadorCambios.Cambio -= OnCambioDatos;

            CargarUsuarios();
            LimpiarFormulario();
        }

        private void AplicarEstilos()
        {
            this.BackColor    = EstiloPos.Fondo;
            pnlForm.BackColor = EstiloPos.Surface;

            lblModo.Font      = EstiloPos.FontSubtitulo;
            lblModo.ForeColor = EstiloPos.Ink1;

            foreach (var l in new[] { lblNombre, lblLogin, lblRol, lblPass })
            {
                l.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
                l.ForeColor = EstiloPos.Ink2;
            }
            lblPassHint.Font      = EstiloPos.FontSmall;
            lblPassHint.ForeColor = EstiloPos.Ink3;

            foreach (var t in new[] { txtNombre, txtLogin, txtPass })
                EstiloPos.AplicarInput(t);
            txtPass.UseSystemPasswordChar = true;   // AplicarInput no lo activa

            EstiloPos.AplicarCombo(cmbRol);
            cmbRol.Size = new Size(150, EstiloPos.AlturaInput);

            chkActivo.Font      = EstiloPos.FontBody;
            chkActivo.ForeColor = EstiloPos.Ink1;
            chkActivo.Cursor    = Cursors.Hand;

            EstiloPos.AplicarBotonPrimario(btnGuardar);
            EstiloPos.AplicarBotonSecundario(btnCancelar);
            EstiloPos.AplicarBotonSecundario(btnEstado);
            EstiloPos.AplicarBotonSecundario(btnReset);

            lblError.Font      = EstiloPos.FontSmall;
            lblError.ForeColor = EstiloPos.Rojo;
        }

        private void OnCambioDatos(string entidad)
        {
            if (IsDisposed || Disposing) return;
            if (entidad == Entidad.Usuario) CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            dgv.SuspendLayout();
            dgv.Rows.Clear();
            foreach (var u in usuarioService.ObtenerTodos())
            {
                int fila = dgv.Rows.Add(u.IdUsuario, u.Nombre, u.LoginNombre, u.Rol,
                    u.Activo ? "Activo" : "Inactivo", u.DebeCambiarPassword ? "Sí" : "—");
                if (!u.Activo)
                {
                    var row = dgv.Rows[fila];
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(160, 160, 160);
                    row.DefaultCellStyle.SelectionForeColor = Color.FromArgb(140, 140, 140);
                }
            }
            dgv.ResumeLayout();
            ActualizarBotonEstado();
        }

        private void ActualizarBotonEstado()
        {
            if (dgv.CurrentRow == null) { btnEstado.Text = "Desactivar"; return; }
            bool inactivo = dgv.CurrentRow.Cells["colEstado"].Value?.ToString() == "Inactivo";
            btnEstado.Text = inactivo ? "Activar" : "Desactivar";
            btnEstado.ForeColor = inactivo ? EstiloPos.Verde : EstiloPos.Ink2;
        }

        private void dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["colId"].Value);
            var u = usuarioService.ObtenerPorId(id);
            if (u == null) return;

            idEnEdicion        = u.IdUsuario;
            txtNombre.Text     = u.Nombre;
            txtLogin.Text      = u.LoginNombre;
            cmbRol.SelectedItem = u.Rol;
            chkActivo.Checked  = u.Activo;

            lblPass.Visible     = false;   // la contraseña no se edita aquí (se resetea)
            txtPass.Visible     = false;
            lblPassHint.Visible = true;

            lblModo.Text        = "Editando: " + u.LoginNombre;
            btnGuardar.Text     = "Actualizar";
            btnCancelar.Visible = true;
            lblError.Visible    = false;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            var u = new Usuario
            {
                IdUsuario   = idEnEdicion,
                Nombre      = txtNombre.Text.Trim(),
                LoginNombre = txtLogin.Text.Trim(),
                Rol         = cmbRol.SelectedItem != null ? cmbRol.SelectedItem.ToString() : RolUsuario.Cajero,
                Activo      = chkActivo.Checked
            };
            try
            {
                if (idEnEdicion == 0)
                {
                    usuarioService.Crear(u, txtPass.Text);
                    Aviso.Exito(this, "Usuario «" + u.LoginNombre + "» creado.\nDeberá cambiar la contraseña en su primer ingreso.",
                        "Usuario creado");
                }
                else
                {
                    if (!Aviso.Confirmar(this, "Se guardarán los cambios de «" + u.LoginNombre + "».",
                            "¿Actualizar usuario?", "Actualizar", TipoAviso.Info))
                        return;
                    usuarioService.Actualizar(u);
                    Aviso.Exito(this, "Los cambios de «" + u.LoginNombre + "» se guardaron correctamente.",
                        "Usuario actualizado");
                }
                LimpiarFormulario();
            }
            catch (Exception ex) { Errores.Mostrar(this, ex); }
        }

        private void btnEstado_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) { MostrarError("Selecciona un usuario en la tabla."); return; }
            int    id    = Convert.ToInt32(dgv.CurrentRow.Cells["colId"].Value);
            string login = dgv.CurrentRow.Cells["colLogin"].Value?.ToString();
            bool inactivo = dgv.CurrentRow.Cells["colEstado"].Value?.ToString() == "Inactivo";
            try
            {
                if (inactivo)
                {
                    if (Aviso.Confirmar(this, "El usuario podrá volver a iniciar sesión.",
                            "¿Activar «" + login + "»?", "Activar", TipoAviso.Info))
                        usuarioService.Activar(id);
                }
                else
                {
                    if (Aviso.Confirmar(this, "No podrá iniciar sesión hasta reactivarlo.",
                            "¿Desactivar «" + login + "»?", "Desactivar"))
                        usuarioService.Desactivar(id);
                }
            }
            catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo"); }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) { MostrarError("Selecciona un usuario en la tabla."); return; }
            int    id    = Convert.ToInt32(dgv.CurrentRow.Cells["colId"].Value);
            string login = dgv.CurrentRow.Cells["colLogin"].Value?.ToString();

            while (true)
            {
                using (var dlg = new FormCambiarPassword("Resetear contraseña",
                    "Define una contraseña temporal para «" + login + "». Deberá cambiarla en su próximo ingreso.",
                    pedirActual: false, obligatorio: false))
                {
                    if (dlg.ShowDialog(this) != DialogResult.OK) return;
                    try
                    {
                        usuarioService.ResetearPassword(id, dlg.PasswordNueva);
                        Aviso.Exito(this, "Contraseña reseteada para «" + login + "».", "Listo");
                        return;
                    }
                    catch (Exception ex) { Aviso.Error(this, Errores.Usuario(ex), "No se pudo"); }
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5: CargarUsuarios(); return true;
                case Keys.Escape: LimpiarFormulario(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void LimpiarFormulario()
        {
            idEnEdicion = 0;
            txtNombre.Clear();
            txtLogin.Clear();
            txtPass.Clear();
            cmbRol.SelectedIndex = cmbRol.Items.Count > 0 ? 0 : -1;
            chkActivo.Checked = true;

            lblPass.Visible     = true;
            txtPass.Visible     = true;
            lblPassHint.Visible = false;

            lblModo.Text        = "Nuevo usuario";
            btnGuardar.Text     = "Guardar";
            btnCancelar.Visible = false;
            lblError.Visible    = false;
            txtNombre.Focus();
        }

        private void MostrarError(string msg) { lblError.Text = msg; lblError.Visible = true; }
    }
}
