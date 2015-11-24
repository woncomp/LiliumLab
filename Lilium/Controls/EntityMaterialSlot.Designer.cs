namespace Lilium.Controls
{
	partial class EntityMaterialSlot
	{
		/// <summary> 
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region 组件设计器生成的代码

		/// <summary> 
		/// 设计器支持所需的方法 - 不要
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.btnMaterialName = new System.Windows.Forms.Button();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.labelTitle = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnMaterialName
			// 
			this.btnMaterialName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnMaterialName.Location = new System.Drawing.Point(75, 0);
			this.btnMaterialName.Name = "btnMaterialName";
			this.btnMaterialName.Size = new System.Drawing.Size(194, 20);
			this.btnMaterialName.TabIndex = 15;
			this.btnMaterialName.Text = "Material Name";
			this.btnMaterialName.UseVisualStyleBackColor = true;
			this.btnMaterialName.Click += new System.EventHandler(this.btnMaterialName_Click);
			// 
			// btnBrowse
			// 
			this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowse.Location = new System.Drawing.Point(296, 0);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(64, 20);
			this.btnBrowse.TabIndex = 14;
			this.btnBrowse.Text = "Browse";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// labelTitle
			// 
			this.labelTitle.Location = new System.Drawing.Point(0, 0);
			this.labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.labelTitle.MaximumSize = new System.Drawing.Size(9999, 9999);
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.Size = new System.Drawing.Size(72, 20);
			this.labelTitle.TabIndex = 13;
			this.labelTitle.Text = "Material 00";
			this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// EntityMaterialSlot
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnMaterialName);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.labelTitle);
			this.Name = "EntityMaterialSlot";
			this.Size = new System.Drawing.Size(360, 20);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnMaterialName;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label labelTitle;
	}
}
