namespace RevitMasterUI
{
    partial class FormExporter
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_ChangeRevitPath = new System.Windows.Forms.Button();
            this.tb_RevitPath = new System.Windows.Forms.TextBox();
            this.tb_FilePath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_ChangeFilePath = new System.Windows.Forms.Button();
            this.btnIfcConfig = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_OK
            // 
            this.btn_OK.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_OK.Location = new System.Drawing.Point(299, 80);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(75, 25);
            this.btn_OK.TabIndex = 0;
            this.btn_OK.Text = "OK";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Cancel.Location = new System.Drawing.Point(380, 80);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 25);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_ChangeRevitPath
            // 
            this.btn_ChangeRevitPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_ChangeRevitPath.Location = new System.Drawing.Point(423, 20);
            this.btn_ChangeRevitPath.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_ChangeRevitPath.Name = "btn_ChangeRevitPath";
            this.btn_ChangeRevitPath.Size = new System.Drawing.Size(32, 21);
            this.btn_ChangeRevitPath.TabIndex = 2;
            this.btn_ChangeRevitPath.Text = "...";
            this.btn_ChangeRevitPath.UseVisualStyleBackColor = true;
            this.btn_ChangeRevitPath.Click += new System.EventHandler(this.btn_ChangeRevitPath_Click);
            // 
            // tb_RevitPath
            // 
            this.tb_RevitPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_RevitPath.Location = new System.Drawing.Point(121, 19);
            this.tb_RevitPath.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_RevitPath.Name = "tb_RevitPath";
            this.tb_RevitPath.Size = new System.Drawing.Size(297, 22);
            this.tb_RevitPath.TabIndex = 4;
            // 
            // tb_FilePath
            // 
            this.tb_FilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_FilePath.Location = new System.Drawing.Point(120, 47);
            this.tb_FilePath.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_FilePath.Name = "tb_FilePath";
            this.tb_FilePath.Size = new System.Drawing.Size(297, 22);
            this.tb_FilePath.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(44, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Revit path:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(16, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "Revit files path:";
            // 
            // btn_ChangeFilePath
            // 
            this.btn_ChangeFilePath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_ChangeFilePath.Location = new System.Drawing.Point(423, 48);
            this.btn_ChangeFilePath.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btn_ChangeFilePath.Name = "btn_ChangeFilePath";
            this.btn_ChangeFilePath.Size = new System.Drawing.Size(32, 21);
            this.btn_ChangeFilePath.TabIndex = 8;
            this.btn_ChangeFilePath.Text = "...";
            this.btn_ChangeFilePath.UseVisualStyleBackColor = true;
            this.btn_ChangeFilePath.Click += new System.EventHandler(this.btn_ChangeFilePath_Click);
            // 
            // btnIfcConfig
            // 
            this.btnIfcConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIfcConfig.Location = new System.Drawing.Point(218, 80);
            this.btnIfcConfig.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnIfcConfig.Name = "btnIfcConfig";
            this.btnIfcConfig.Size = new System.Drawing.Size(75, 25);
            this.btnIfcConfig.TabIndex = 9;
            this.btnIfcConfig.Text = "Setting";
            this.btnIfcConfig.UseVisualStyleBackColor = true;
            this.btnIfcConfig.Click += new System.EventHandler(this.btnIfcConfig_Click);
            // 
            // FormExporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(474, 116);
            this.Controls.Add(this.btnIfcConfig);
            this.Controls.Add(this.btn_ChangeFilePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_FilePath);
            this.Controls.Add(this.tb_RevitPath);
            this.Controls.Add(this.btn_ChangeRevitPath);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.Name = "FormExporter";
            this.Text = "IFC Exporter";
            this.Load += new System.EventHandler(this.FormExporter_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_ChangeRevitPath;
        private System.Windows.Forms.TextBox tb_RevitPath;
        private System.Windows.Forms.TextBox tb_FilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_ChangeFilePath;
        private System.Windows.Forms.Button btnIfcConfig;
    }
}

