namespace Lilium
{
	partial class MaterialEditor
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.label1 = new System.Windows.Forms.Label();
			this.btnBrowseShader = new System.Windows.Forms.Button();
			this.tbShaderFile = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.propertyGrid1.CategoryForeColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.propertyGrid1.Location = new System.Drawing.Point(0, 33);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(862, 566);
			this.propertyGrid1.TabIndex = 0;
			this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(65, 12);
			this.label1.TabIndex = 1;
			this.label1.Text = "ShaderFile";
			// 
			// btnBrowseShader
			// 
			this.btnBrowseShader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowseShader.Location = new System.Drawing.Point(775, 4);
			this.btnBrowseShader.Name = "btnBrowseShader";
			this.btnBrowseShader.Size = new System.Drawing.Size(75, 23);
			this.btnBrowseShader.TabIndex = 2;
			this.btnBrowseShader.Text = "Browse";
			this.btnBrowseShader.UseVisualStyleBackColor = true;
			this.btnBrowseShader.Click += new System.EventHandler(this.btnBrowseShader_Click);
			// 
			// tbShaderFile
			// 
			this.tbShaderFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tbShaderFile.Location = new System.Drawing.Point(83, 6);
			this.tbShaderFile.Name = "tbShaderFile";
			this.tbShaderFile.Size = new System.Drawing.Size(686, 21);
			this.tbShaderFile.TabIndex = 3;
			// 
			// MaterialEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(862, 599);
			this.Controls.Add(this.tbShaderFile);
			this.Controls.Add(this.btnBrowseShader);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.propertyGrid1);
			this.Name = "MaterialEditor";
			this.Text = "MaterialEditor";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnBrowseShader;
		private System.Windows.Forms.TextBox tbShaderFile;
	}
}