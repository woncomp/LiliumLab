namespace Lilium.Controls
{
	partial class PassHeader
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
			this.labelTitle = new System.Windows.Forms.Label();
			this.labelShader = new System.Windows.Forms.Label();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.btnRasterizer = new System.Windows.Forms.Button();
			this.btnBlend = new System.Windows.Forms.Button();
			this.btnDepthStencil = new System.Windows.Forms.Button();
			this.btnEntries = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// labelTitle
			// 
			this.labelTitle.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.labelTitle.Location = new System.Drawing.Point(0, 0);
			this.labelTitle.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.labelTitle.MaximumSize = new System.Drawing.Size(9999, 9999);
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.Size = new System.Drawing.Size(87, 20);
			this.labelTitle.TabIndex = 6;
			this.labelTitle.Text = "< Pass 0 >";
			this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelShader
			// 
			this.labelShader.Location = new System.Drawing.Point(3, 0);
			this.labelShader.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.labelShader.MaximumSize = new System.Drawing.Size(9999, 9999);
			this.labelShader.Name = "labelShader";
			this.labelShader.Size = new System.Drawing.Size(256, 20);
			this.labelShader.TabIndex = 7;
			this.labelShader.Text = "ShaderFile.hlsl";
			this.labelShader.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnBrowse
			// 
			this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBrowse.Location = new System.Drawing.Point(262, 0);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(98, 22);
			this.btnBrowse.TabIndex = 8;
			this.btnBrowse.Text = "Browse Shader";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// btnRasterizer
			// 
			this.btnRasterizer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnRasterizer.Location = new System.Drawing.Point(0, 23);
			this.btnRasterizer.Name = "btnRasterizer";
			this.btnRasterizer.Size = new System.Drawing.Size(88, 22);
			this.btnRasterizer.TabIndex = 9;
			this.btnRasterizer.Text = "Rasterizer";
			this.btnRasterizer.UseVisualStyleBackColor = true;
			this.btnRasterizer.Click += new System.EventHandler(this.btnRasterizer_Click);
			// 
			// btnBlend
			// 
			this.btnBlend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnBlend.Location = new System.Drawing.Point(91, 23);
			this.btnBlend.Margin = new System.Windows.Forms.Padding(0);
			this.btnBlend.Name = "btnBlend";
			this.btnBlend.Size = new System.Drawing.Size(71, 22);
			this.btnBlend.TabIndex = 10;
			this.btnBlend.Text = "Blend";
			this.btnBlend.UseVisualStyleBackColor = true;
			this.btnBlend.Click += new System.EventHandler(this.btnBlend_Click);
			// 
			// btnDepthStencil
			// 
			this.btnDepthStencil.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDepthStencil.Location = new System.Drawing.Point(165, 23);
			this.btnDepthStencil.Margin = new System.Windows.Forms.Padding(0);
			this.btnDepthStencil.Name = "btnDepthStencil";
			this.btnDepthStencil.Size = new System.Drawing.Size(94, 22);
			this.btnDepthStencil.TabIndex = 11;
			this.btnDepthStencil.Text = "DepthStencil";
			this.btnDepthStencil.UseVisualStyleBackColor = true;
			this.btnDepthStencil.Click += new System.EventHandler(this.btnDepthStencil_Click);
			// 
			// btnEntries
			// 
			this.btnEntries.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnEntries.Location = new System.Drawing.Point(262, 23);
			this.btnEntries.Name = "btnEntries";
			this.btnEntries.Size = new System.Drawing.Size(98, 22);
			this.btnEntries.TabIndex = 12;
			this.btnEntries.Text = "Entry Point";
			this.btnEntries.UseVisualStyleBackColor = true;
			this.btnEntries.Click += new System.EventHandler(this.btnEntries_Click);
			// 
			// PassHeader
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnEntries);
			this.Controls.Add(this.btnDepthStencil);
			this.Controls.Add(this.btnBlend);
			this.Controls.Add(this.btnRasterizer);
			this.Controls.Add(this.labelTitle);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.labelShader);
			this.Name = "PassHeader";
			this.Size = new System.Drawing.Size(360, 45);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelTitle;
		private System.Windows.Forms.Label labelShader;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Button btnRasterizer;
		private System.Windows.Forms.Button btnBlend;
		private System.Windows.Forms.Button btnDepthStencil;
		private System.Windows.Forms.Button btnEntries;
	}
}
