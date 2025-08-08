namespace DbAutoChat.Win.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem refreshSchemaToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel connectionStatusLabel;
        private ToolStripStatusLabel queryStatusLabel;
        private ToolStripStatusLabel rowCountLabel;
        private ToolStripProgressBar progressBar;
        private SplitContainer mainSplitContainer;
        private SplitContainer leftSplitContainer;
        private Panel conversationPanel;
        private RichTextBox conversationRichTextBox;
        private Panel inputPanel;
        private TextBox inputTextBox;
        private Button askButton;
        private Label securityBannerLabel;
        private Panel resultsPanel;
        private DataGridView resultsDataGridView;
        private ContextMenuStrip resultsContextMenu;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem exportCsvToolStripMenuItem;

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
            components = new System.ComponentModel.Container();
            
            // Initialize all controls
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            refreshSchemaToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            connectionStatusLabel = new ToolStripStatusLabel();
            queryStatusLabel = new ToolStripStatusLabel();
            rowCountLabel = new ToolStripStatusLabel();
            progressBar = new ToolStripProgressBar();
            mainSplitContainer = new SplitContainer();
            leftSplitContainer = new SplitContainer();
            conversationPanel = new Panel();
            conversationRichTextBox = new RichTextBox();
            inputPanel = new Panel();
            inputTextBox = new TextBox();
            askButton = new Button();
            securityBannerLabel = new Label();
            resultsPanel = new Panel();
            resultsDataGridView = new DataGridView();
            resultsContextMenu = new ContextMenuStrip(components);
            copyToolStripMenuItem = new ToolStripMenuItem();
            exportCsvToolStripMenuItem = new ToolStripMenuItem();

            SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)leftSplitContainer).BeginInit();
            leftSplitContainer.Panel1.SuspendLayout();
            leftSplitContainer.Panel2.SuspendLayout();
            leftSplitContainer.SuspendLayout();
            conversationPanel.SuspendLayout();
            inputPanel.SuspendLayout();
            resultsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)resultsDataGridView).BeginInit();
            resultsContextMenu.SuspendLayout();
            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();

            // MenuStrip
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, toolsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1200, 24);
            menuStrip.TabIndex = 0;

            // File Menu
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";

            // Tools Menu
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { settingsToolStripMenuItem, refreshSchemaToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(46, 20);
            toolsToolStripMenuItem.Text = "&Tools";

            // Settings MenuItem
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(180, 22);
            settingsToolStripMenuItem.Text = "&Settings...";
            settingsToolStripMenuItem.Click += SettingsToolStripMenuItem_Click;

            // Refresh Schema MenuItem
            refreshSchemaToolStripMenuItem.Name = "refreshSchemaToolStripMenuItem";
            refreshSchemaToolStripMenuItem.Size = new Size(180, 22);
            refreshSchemaToolStripMenuItem.Text = "&Refresh Schema";
            refreshSchemaToolStripMenuItem.Click += RefreshSchemaToolStripMenuItem_Click;

            // Help Menu
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";

            // About MenuItem
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(180, 22);
            aboutToolStripMenuItem.Text = "&About...";
            aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;

            // StatusStrip
            statusStrip.Items.AddRange(new ToolStripItem[] { connectionStatusLabel, queryStatusLabel, rowCountLabel, progressBar });
            statusStrip.Location = new Point(0, 776);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 24);
            statusStrip.TabIndex = 1;

            // Connection Status Label
            connectionStatusLabel.Name = "connectionStatusLabel";
            connectionStatusLabel.Size = new Size(79, 19);
            connectionStatusLabel.Text = "Disconnected";

            // Query Status Label
            queryStatusLabel.Name = "queryStatusLabel";
            queryStatusLabel.Size = new Size(39, 19);
            queryStatusLabel.Text = "Ready";

            // Row Count Label
            rowCountLabel.Name = "rowCountLabel";
            rowCountLabel.Size = new Size(0, 19);

            // Progress Bar
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(100, 18);
            progressBar.Visible = false;

            // Main Split Container
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.Location = new Point(0, 24);
            mainSplitContainer.Name = "mainSplitContainer";
            mainSplitContainer.Orientation = Orientation.Vertical;
            mainSplitContainer.Panel1.Controls.Add(leftSplitContainer);
            mainSplitContainer.Panel2.Controls.Add(resultsPanel);
            mainSplitContainer.Size = new Size(1200, 752);
            mainSplitContainer.SplitterDistance = 400;
            mainSplitContainer.TabIndex = 2;

            // Left Split Container (Conversation + Input)
            leftSplitContainer.Dock = DockStyle.Fill;
            leftSplitContainer.Location = new Point(0, 0);
            leftSplitContainer.Name = "leftSplitContainer";
            leftSplitContainer.Orientation = Orientation.Horizontal;
            leftSplitContainer.Panel1.Controls.Add(conversationPanel);
            leftSplitContainer.Panel2.Controls.Add(inputPanel);
            leftSplitContainer.Size = new Size(1200, 400);
            leftSplitContainer.SplitterDistance = 320;
            leftSplitContainer.TabIndex = 0;

            // Conversation Panel
            conversationPanel.Controls.Add(conversationRichTextBox);
            conversationPanel.Dock = DockStyle.Fill;
            conversationPanel.Location = new Point(0, 0);
            conversationPanel.Name = "conversationPanel";
            conversationPanel.Size = new Size(1200, 320);
            conversationPanel.TabIndex = 0;

            // Conversation RichTextBox
            conversationRichTextBox.BackColor = Color.White;
            conversationRichTextBox.Dock = DockStyle.Fill;
            conversationRichTextBox.Font = new Font("Segoe UI", 9F);
            conversationRichTextBox.Location = new Point(0, 0);
            conversationRichTextBox.Name = "conversationRichTextBox";
            conversationRichTextBox.ReadOnly = true;
            conversationRichTextBox.Size = new Size(1200, 320);
            conversationRichTextBox.TabIndex = 0;
            conversationRichTextBox.Text = "";

            // Input Panel
            inputPanel.Controls.Add(securityBannerLabel);
            inputPanel.Controls.Add(askButton);
            inputPanel.Controls.Add(inputTextBox);
            inputPanel.Dock = DockStyle.Fill;
            inputPanel.Location = new Point(0, 0);
            inputPanel.Name = "inputPanel";
            inputPanel.Padding = new Padding(8);
            inputPanel.Size = new Size(1200, 76);
            inputPanel.TabIndex = 0;

            // Security Banner Label
            securityBannerLabel.BackColor = Color.LightYellow;
            securityBannerLabel.BorderStyle = BorderStyle.FixedSingle;
            securityBannerLabel.Dock = DockStyle.Top;
            securityBannerLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            securityBannerLabel.ForeColor = Color.DarkOrange;
            securityBannerLabel.Location = new Point(8, 8);
            securityBannerLabel.Name = "securityBannerLabel";
            securityBannerLabel.Size = new Size(1184, 20);
            securityBannerLabel.TabIndex = 2;
            securityBannerLabel.Text = "🔒 SELECT-only enforced - Read-only database access";
            securityBannerLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Input TextBox
            inputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputTextBox.Font = new Font("Segoe UI", 10F);
            inputTextBox.Location = new Point(8, 36);
            inputTextBox.Name = "inputTextBox";
            inputTextBox.PlaceholderText = "Ask a question about your database (English or Hinglish)...";
            inputTextBox.Size = new Size(1094, 25);
            inputTextBox.TabIndex = 0;
            inputTextBox.KeyDown += InputTextBox_KeyDown;

            // Ask Button
            askButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            askButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            askButton.Location = new Point(1108, 36);
            askButton.Name = "askButton";
            askButton.Size = new Size(75, 25);
            askButton.TabIndex = 1;
            askButton.Text = "Ask";
            askButton.UseVisualStyleBackColor = true;
            askButton.Click += AskButton_Click;

            // Results Panel
            resultsPanel.Controls.Add(resultsDataGridView);
            resultsPanel.Dock = DockStyle.Fill;
            resultsPanel.Location = new Point(0, 0);
            resultsPanel.Name = "resultsPanel";
            resultsPanel.Size = new Size(1200, 348);
            resultsPanel.TabIndex = 0;

            // Results DataGridView
            resultsDataGridView.AllowUserToAddRows = false;
            resultsDataGridView.AllowUserToDeleteRows = false;
            resultsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            resultsDataGridView.BackgroundColor = Color.White;
            resultsDataGridView.ContextMenuStrip = resultsContextMenu;
            resultsDataGridView.Dock = DockStyle.Fill;
            resultsDataGridView.Location = new Point(0, 0);
            resultsDataGridView.Name = "resultsDataGridView";
            resultsDataGridView.ReadOnly = true;
            resultsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            resultsDataGridView.Size = new Size(1200, 348);
            resultsDataGridView.TabIndex = 0;

            // Results Context Menu
            resultsContextMenu.Items.AddRange(new ToolStripItem[] { copyToolStripMenuItem, exportCsvToolStripMenuItem });
            resultsContextMenu.Name = "resultsContextMenu";
            resultsContextMenu.Size = new Size(181, 70);

            // Copy MenuItem
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new Size(180, 22);
            copyToolStripMenuItem.Text = "&Copy Selected";
            copyToolStripMenuItem.Click += CopyToolStripMenuItem_Click;

            // Export CSV MenuItem
            exportCsvToolStripMenuItem.Name = "exportCsvToolStripMenuItem";
            exportCsvToolStripMenuItem.Size = new Size(180, 22);
            exportCsvToolStripMenuItem.Text = "&Export to CSV...";
            exportCsvToolStripMenuItem.Click += ExportCsvToolStripMenuItem_Click;

            // MainForm
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(mainSplitContainer);
            Controls.Add(statusStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            Text = "DbAutoChat.Win";

            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            leftSplitContainer.Panel1.ResumeLayout(false);
            leftSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)leftSplitContainer).EndInit();
            leftSplitContainer.ResumeLayout(false);
            conversationPanel.ResumeLayout(false);
            inputPanel.ResumeLayout(false);
            inputPanel.PerformLayout();
            resultsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)resultsDataGridView).EndInit();
            resultsContextMenu.ResumeLayout(false);
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}