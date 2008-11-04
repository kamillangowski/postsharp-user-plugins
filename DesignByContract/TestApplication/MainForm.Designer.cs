namespace TestApplication
{
	partial class MainForm
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnAspectTest = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btnAspectTest
			// 
			this.btnAspectTest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.btnAspectTest.Location = new System.Drawing.Point(12, 12);
			this.btnAspectTest.Name = "btnAspectTest";
			this.btnAspectTest.Size = new System.Drawing.Size(268, 23);
			this.btnAspectTest.TabIndex = 0;
			this.btnAspectTest.Text = "Aspect Tests";
			this.btnAspectTest.UseVisualStyleBackColor = true;
			this.btnAspectTest.Click += new System.EventHandler(this.btnAspectTest_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 46);
			this.Controls.Add(this.btnAspectTest);
			this.Name = "MainForm";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnAspectTest;
	}
}

