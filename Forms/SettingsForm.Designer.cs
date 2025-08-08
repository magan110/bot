namespace DbAutoChat.Win.Forms
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private TabControl tabControl;
        private TabPage databaseTabPage;
        private TabPage llmTabPage;
        private TabPage generalTabPage;
        private Label connectionStringLabel;
        private TextBox connectionStringTextBox;
        private Button testConnectionButton;
        private Label providerLabel;
        private ComboBox providerComboBox;
        private Panel openAiPanel;
        private Label openAiApiKeyLabel;
        private TextBox openAiApiKeyTextBox;
        private Panel geminiPanel;
        private Label geminiApiKeyLabel;
        private TextBox geminiApiKeyTextBox;
        private Panel ollamaPanel;
        private Label ollamaUrlLabel;
        private TextBox ollamaUrlTextBox;
        private Label maxRowsLabel;
        private NumericUpDown maxRowsNumericUpDown;
        private Button saveButton;
        private Button cancelButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            tabControl = new TabControl();
            databaseTabPage = new TabPage();
            llmTabPage = new TabPage();
            generalTabPage = new TabPage();
            connectionStringLabel = new Label();
            connectionStringTextBox = new TextBox();
            testConnectionButton = new Button();
            providerLabel = new Label();
            providerComboBox = new ComboBox();
            openAiPanel = new Panel();
            openAiApiKeyLabel = new Label();
            openAiApiKeyTextBox = new TextBox();
            geminiPanel = new Panel();
            geminiApiKeyLabel = new Label();
            geminiApiKeyTextBox = new TextBox();
            ollamaPanel = new Panel();
            ollamaUrlLabel = new Label();
            ollamaUrlTextBox = new TextBox();
            maxRowsLabel = new Label();
            maxRowsNumericUpDown = new NumericUpDown();
            saveButton = new Button();
            cancelButton = new Button();

            SuspendLayout();
            tabControl.SuspendLayout();
            databaseTabPage.SuspendLayout();
            llmTabPage.SuspendLayout();
            generalTabPage.SuspendLayout();
            openAiPanel.SuspendLayout();
            geminiPanel.SuspendLayout();
            ollamaPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)maxRowsNumericUpDown).BeginInit();

            // TabControl
            tabControl.Controls.Add(databaseTabPage);
            tabControl.Controls.Add(llmTabPage);
            tabControl.Controls.Add(generalTabPage);
            tabControl.Location = new Point(12, 12);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(560, 350);
            tabControl.TabIndex = 0;

            // Database Tab Page
            databaseTabPage.Controls.Add(testConnectionButton);
            databaseTabPage.Controls.Add(connectionStringTextBox);
            databaseTabPage.Controls.Add(connectionStringLabel);
            databaseTabPage.Location = new Point(4, 24);
            databaseTabPage.Name = "databaseTabPage";
            databaseTabPage.Padding = new Padding(3);
            databaseTabPage.Size = new Size(552, 322);
            databaseTabPage.TabIndex = 0;
            databaseTabPage.Text = "Database";
            databaseTabPage.UseVisualStyleBackColor = true;

            // Connection String Label
            connectionStringLabel.AutoSize = true;
            connectionStringLabel.Location = new Point(15, 20);
            connectionStringLabel.Name = "connectionStringLabel";
            connectionStringLabel.Size = new Size(104, 15);
            connectionStringLabel.TabIndex = 0;
            connectionStringLabel.Text = "Connection String:";

            // Connection String TextBox
            connectionStringTextBox.Location = new Point(15, 45);
            connectionStringTextBox.Multiline = true;
            connectionStringTextBox.Name = "connectionStringTextBox";
            connectionStringTextBox.Size = new Size(520, 80);
            connectionStringTextBox.TabIndex = 1;
            connectionStringTextBox.PlaceholderText = "Server=localhost;Database=YourDatabase;Integrated Security=true;TrustServerCertificate=true;";

            // Test Connection Button
            testConnectionButton.Location = new Point(15, 140);
            testConnectionButton.Name = "testConnectionButton";
            testConnectionButton.Size = new Size(120, 30);
            testConnectionButton.TabIndex = 2;
            testConnectionButton.Text = "Test Connection";
            testConnectionButton.UseVisualStyleBackColor = true;
            testConnectionButton.Click += TestConnectionButton_Click;

            // LLM Tab Page
            llmTabPage.Controls.Add(ollamaPanel);
            llmTabPage.Controls.Add(geminiPanel);
            llmTabPage.Controls.Add(openAiPanel);
            llmTabPage.Controls.Add(providerComboBox);
            llmTabPage.Controls.Add(providerLabel);
            llmTabPage.Location = new Point(4, 24);
            llmTabPage.Name = "llmTabPage";
            llmTabPage.Padding = new Padding(3);
            llmTabPage.Size = new Size(552, 322);
            llmTabPage.TabIndex = 1;
            llmTabPage.Text = "LLM Provider";
            llmTabPage.UseVisualStyleBackColor = true;

            // Provider Label
            providerLabel.AutoSize = true;
            providerLabel.Location = new Point(15, 20);
            providerLabel.Name = "providerLabel";
            providerLabel.Size = new Size(55, 15);
            providerLabel.TabIndex = 0;
            providerLabel.Text = "Provider:";

            // Provider ComboBox
            providerComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            providerComboBox.Items.AddRange(new object[] { "OpenAI", "Gemini", "Ollama" });
            providerComboBox.Location = new Point(15, 45);
            providerComboBox.Name = "providerComboBox";
            providerComboBox.Size = new Size(200, 23);
            providerComboBox.TabIndex = 1;
            providerComboBox.SelectedIndex = 0;
            providerComboBox.SelectedIndexChanged += ProviderComboBox_SelectedIndexChanged;

            // OpenAI Panel
            openAiPanel.Controls.Add(openAiApiKeyTextBox);
            openAiPanel.Controls.Add(openAiApiKeyLabel);
            openAiPanel.Location = new Point(15, 85);
            openAiPanel.Name = "openAiPanel";
            openAiPanel.Size = new Size(520, 80);
            openAiPanel.TabIndex = 2;

            // OpenAI API Key Label
            openAiApiKeyLabel.AutoSize = true;
            openAiApiKeyLabel.Location = new Point(0, 10);
            openAiApiKeyLabel.Name = "openAiApiKeyLabel";
            openAiApiKeyLabel.Size = new Size(88, 15);
            openAiApiKeyLabel.TabIndex = 0;
            openAiApiKeyLabel.Text = "OpenAI API Key:";

            // OpenAI API Key TextBox
            openAiApiKeyTextBox.Location = new Point(0, 35);
            openAiApiKeyTextBox.Name = "openAiApiKeyTextBox";
            openAiApiKeyTextBox.PasswordChar = '*';
            openAiApiKeyTextBox.Size = new Size(400, 23);
            openAiApiKeyTextBox.TabIndex = 1;
            openAiApiKeyTextBox.PlaceholderText = "sk-...";

            // Gemini Panel
            geminiPanel.Controls.Add(geminiApiKeyTextBox);
            geminiPanel.Controls.Add(geminiApiKeyLabel);
            geminiPanel.Location = new Point(15, 85);
            geminiPanel.Name = "geminiPanel";
            geminiPanel.Size = new Size(520, 80);
            geminiPanel.TabIndex = 3;
            geminiPanel.Visible = false;

            // Gemini API Key Label
            geminiApiKeyLabel.AutoSize = true;
            geminiApiKeyLabel.Location = new Point(0, 10);
            geminiApiKeyLabel.Name = "geminiApiKeyLabel";
            geminiApiKeyLabel.Size = new Size(95, 15);
            geminiApiKeyLabel.TabIndex = 0;
            geminiApiKeyLabel.Text = "Gemini API Key:";

            // Gemini API Key TextBox
            geminiApiKeyTextBox.Location = new Point(0, 35);
            geminiApiKeyTextBox.Name = "geminiApiKeyTextBox";
            geminiApiKeyTextBox.PasswordChar = '*';
            geminiApiKeyTextBox.Size = new Size(400, 23);
            geminiApiKeyTextBox.TabIndex = 1;
            geminiApiKeyTextBox.PlaceholderText = "AIza...";

            // Ollama Panel
            ollamaPanel.Controls.Add(ollamaUrlTextBox);
            ollamaPanel.Controls.Add(ollamaUrlLabel);
            ollamaPanel.Location = new Point(15, 85);
            ollamaPanel.Name = "ollamaPanel";
            ollamaPanel.Size = new Size(520, 80);
            ollamaPanel.TabIndex = 4;
            ollamaPanel.Visible = false;

            // Ollama URL Label
            ollamaUrlLabel.AutoSize = true;
            ollamaUrlLabel.Location = new Point(0, 10);
            ollamaUrlLabel.Name = "ollamaUrlLabel";
            ollamaUrlLabel.Size = new Size(71, 15);
            ollamaUrlLabel.TabIndex = 0;
            ollamaUrlLabel.Text = "Ollama URL:";

            // Ollama URL TextBox
            ollamaUrlTextBox.Location = new Point(0, 35);
            ollamaUrlTextBox.Name = "ollamaUrlTextBox";
            ollamaUrlTextBox.Size = new Size(400, 23);
            ollamaUrlTextBox.TabIndex = 1;
            ollamaUrlTextBox.Text = "http://localhost:11434";

            // General Tab Page
            generalTabPage.Controls.Add(maxRowsNumericUpDown);
            generalTabPage.Controls.Add(maxRowsLabel);
            generalTabPage.Location = new Point(4, 24);
            generalTabPage.Name = "generalTabPage";
            generalTabPage.Padding = new Padding(3);
            generalTabPage.Size = new Size(552, 322);
            generalTabPage.TabIndex = 2;
            generalTabPage.Text = "General";
            generalTabPage.UseVisualStyleBackColor = true;

            // Max Rows Label
            maxRowsLabel.AutoSize = true;
            maxRowsLabel.Location = new Point(15, 20);
            maxRowsLabel.Name = "maxRowsLabel";
            maxRowsLabel.Size = new Size(134, 15);
            maxRowsLabel.TabIndex = 0;
            maxRowsLabel.Text = "Maximum Rows to Return:";

            // Max Rows NumericUpDown
            maxRowsNumericUpDown.Location = new Point(15, 45);
            maxRowsNumericUpDown.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            maxRowsNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            maxRowsNumericUpDown.Name = "maxRowsNumericUpDown";
            maxRowsNumericUpDown.Size = new Size(120, 23);
            maxRowsNumericUpDown.TabIndex = 1;
            maxRowsNumericUpDown.Value = new decimal(new int[] { 1000, 0, 0, 0 });

            // Save Button
            saveButton.Location = new Point(416, 378);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(75, 30);
            saveButton.TabIndex = 1;
            saveButton.Text = "Save";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButton_Click;

            // Cancel Button
            cancelButton.Location = new Point(497, 378);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(75, 30);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;

            // SettingsForm
            AcceptButton = saveButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new Size(584, 420);
            Controls.Add(cancelButton);
            Controls.Add(saveButton);
            Controls.Add(tabControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";

            tabControl.ResumeLayout(false);
            databaseTabPage.ResumeLayout(false);
            databaseTabPage.PerformLayout();
            llmTabPage.ResumeLayout(false);
            llmTabPage.PerformLayout();
            generalTabPage.ResumeLayout(false);
            generalTabPage.PerformLayout();
            openAiPanel.ResumeLayout(false);
            openAiPanel.PerformLayout();
            geminiPanel.ResumeLayout(false);
            geminiPanel.PerformLayout();
            ollamaPanel.ResumeLayout(false);
            ollamaPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)maxRowsNumericUpDown).EndInit();
            ResumeLayout(false);
        }
    }
}