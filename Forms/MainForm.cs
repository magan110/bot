using DbAutoChat.Win.Interfaces;
using DbAutoChat.Win.Services;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;

namespace DbAutoChat.Win.Forms;

public partial class MainForm : Form
{
    private readonly IConfigurationService _configService;
    private readonly IConversationService _conversationService;
    private readonly IDatabaseRepository _databaseRepository;
    private readonly ISqlValidator _sqlValidator;
    private readonly QueryOrchestrator _queryOrchestrator;
    private readonly ILogger<MainForm> _logger;
    private bool _isProcessingQuery = false;

    public MainForm(
        IConfigurationService configService,
        IConversationService conversationService,
        IDatabaseRepository databaseRepository,
        ISqlValidator sqlValidator,
        QueryOrchestrator queryOrchestrator,
        ILogger<MainForm> logger)
    {
        _configService = configService;
        _conversationService = conversationService;
        _databaseRepository = databaseRepository;
        _sqlValidator = sqlValidator;
        _queryOrchestrator = queryOrchestrator;
        _logger = logger;

        InitializeComponent();
        InitializeForm();
    }

    private void InitializeForm()
    {
        Text = "DbAutoChat.Win - Natural Language Database Query Tool";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;

        // Initialize conversation with welcome message
        AddSystemMessage("Welcome to DbAutoChat! Ask me questions about your database in English or Hinglish.");
        AddSystemMessage("Examples: 'Show me all customers' or 'Sabse zyada sales wale customers dikhao'");

        // Check database connection asynchronously after form is shown
        this.Load += async (s, e) => await CheckDatabaseConnectionAsync();

        _logger.LogInformation("MainForm initialized successfully");
    }

    private async Task CheckDatabaseConnectionAsync()
    {
        try
        {
            connectionStatusLabel.Text = "Connecting...";
            var schema = await _databaseRepository.IntrospectSchemaAsync();
            connectionStatusLabel.Text = $"Connected - {schema.Tables.Count} tables";
            connectionStatusLabel.ForeColor = Color.Green;
        }
        catch (Exception ex)
        {
            connectionStatusLabel.Text = "Connection Failed";
            connectionStatusLabel.ForeColor = Color.Red;
            AddSystemMessage($"⚠️ Database connection failed: {ex.Message}");
            _logger.LogError(ex, "Database connection failed");
        }
    }

    private void AddUserMessage(string message)
    {
        AppendToConversation($"👤 You: {message}", Color.Blue, true);
        _conversationService.AddUserMessage(message);
    }

    private void AddSystemMessage(string message)
    {
        AppendToConversation($"🤖 DbAutoChat: {message}", Color.DarkGreen, false);
        _conversationService.AddSystemResponse(message);
    }

    private void AppendToConversation(string message, Color color, bool isUser)
    {
        if (conversationRichTextBox.InvokeRequired)
        {
            conversationRichTextBox.Invoke(() => AppendToConversation(message, color, isUser));
            return;
        }

        conversationRichTextBox.SelectionStart = conversationRichTextBox.TextLength;
        conversationRichTextBox.SelectionLength = 0;
        conversationRichTextBox.SelectionColor = color;
        conversationRichTextBox.SelectionFont = new Font("Segoe UI", 9F, isUser ? FontStyle.Regular : FontStyle.Bold);
        conversationRichTextBox.AppendText($"{message}\n\n");
        conversationRichTextBox.SelectionColor = conversationRichTextBox.ForeColor;
        conversationRichTextBox.ScrollToCaret();
    }

    private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift)
        {
            e.SuppressKeyPress = true;
            await ProcessUserQueryAsync();
        }
    }

    private async void AskButton_Click(object sender, EventArgs e)
    {
        await ProcessUserQueryAsync();
    }

    private async Task ProcessUserQueryAsync()
    {
        if (_isProcessingQuery || string.IsNullOrWhiteSpace(inputTextBox.Text))
            return;

        var userQuery = inputTextBox.Text.Trim();
        inputTextBox.Clear();
        
        _isProcessingQuery = true;
        askButton.Enabled = false;
        inputTextBox.Enabled = false;
        queryStatusLabel.Text = "Processing...";
        progressBar.Visible = true;
        progressBar.Style = ProgressBarStyle.Marquee;

        try
        {
            AddUserMessage(userQuery);

            var context = _conversationService.GetCurrentContext();
            var result = await _queryOrchestrator.ProcessQueryAsync(userQuery, context);

            if (result.Success)
            {
                if (result.Results != null && result.Results.Rows.Count > 0)
                {
                    DisplayResults(result.Results);
                    AddSystemMessage($"✅ Query executed successfully. Found {result.Results.Rows.Count} rows.");
                    rowCountLabel.Text = $"{result.Results.Rows.Count} rows";
                }
                else
                {
                    AddSystemMessage("ℹ️ Query executed successfully but returned no results.");
                    resultsDataGridView.DataSource = null;
                    rowCountLabel.Text = "0 rows";
                }
            }
            else if (result.RequiresClarification)
            {
                AddSystemMessage($"❓ {result.ClarificationQuestion}");
            }
            else
            {
                AddSystemMessage($"❌ {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            AddSystemMessage($"❌ An error occurred: {ex.Message}");
            _logger.LogError(ex, "Error processing user query: {Query}", userQuery);
        }
        finally
        {
            _isProcessingQuery = false;
            askButton.Enabled = true;
            inputTextBox.Enabled = true;
            queryStatusLabel.Text = "Ready";
            progressBar.Visible = false;
            inputTextBox.Focus();
        }
    }

    private void DisplayResults(DataTable dataTable)
    {
        if (resultsDataGridView.InvokeRequired)
        {
            resultsDataGridView.Invoke(() => DisplayResults(dataTable));
            return;
        }

        resultsDataGridView.DataSource = dataTable;
        
        // Auto-size columns but limit maximum width
        foreach (DataGridViewColumn column in resultsDataGridView.Columns)
        {
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            if (column.Width > 200)
            {
                column.Width = 200;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
        }
    }

    private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (resultsDataGridView.SelectedRows.Count == 0)
            return;

        var sb = new StringBuilder();
        
        // Add headers
        var headers = resultsDataGridView.Columns.Cast<DataGridViewColumn>()
            .Select(column => column.HeaderText);
        sb.AppendLine(string.Join("\t", headers));

        // Add selected rows
        foreach (DataGridViewRow row in resultsDataGridView.SelectedRows)
        {
            if (!row.IsNewRow)
            {
                var values = row.Cells.Cast<DataGridViewCell>()
                    .Select(cell => cell.Value?.ToString() ?? "");
                sb.AppendLine(string.Join("\t", values));
            }
        }

        Clipboard.SetText(sb.ToString());
        AddSystemMessage($"📋 Copied {resultsDataGridView.SelectedRows.Count} rows to clipboard.");
    }

    private void ExportCsvToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (resultsDataGridView.DataSource == null)
        {
            MessageBox.Show("No data to export.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var saveDialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = "csv",
            FileName = $"query_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (saveDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                ExportToCsv(saveDialog.FileName);
                AddSystemMessage($"📁 Data exported to: {saveDialog.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.LogError(ex, "Error exporting CSV to {FileName}", saveDialog.FileName);
            }
        }
    }

    private void ExportToCsv(string fileName)
    {
        var dataTable = (DataTable)resultsDataGridView.DataSource;
        var sb = new StringBuilder();

        // Add headers
        var headers = dataTable.Columns.Cast<DataColumn>()
            .Select(column => EscapeCsvField(column.ColumnName));
        sb.AppendLine(string.Join(",", headers));

        // Add data rows
        foreach (DataRow row in dataTable.Rows)
        {
            var values = row.ItemArray.Select(field => EscapeCsvField(field?.ToString() ?? ""));
            sb.AppendLine(string.Join(",", values));
        }

        File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }

    private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        using var settingsForm = new SettingsForm(_configService);
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            // Refresh connection status after settings change
            _ = CheckDatabaseConnectionAsync();
        }
    }

    private async void RefreshSchemaToolStripMenuItem_Click(object sender, EventArgs e)
    {
        refreshSchemaToolStripMenuItem.Enabled = false;
        queryStatusLabel.Text = "Refreshing schema...";
        progressBar.Visible = true;
        progressBar.Style = ProgressBarStyle.Marquee;

        try
        {
            var schema = await _databaseRepository.IntrospectSchemaAsync();
            connectionStatusLabel.Text = $"Connected - {schema.Tables.Count} tables";
            connectionStatusLabel.ForeColor = Color.Green;
            AddSystemMessage($"🔄 Schema refreshed successfully. Found {schema.Tables.Count} tables.");
        }
        catch (Exception ex)
        {
            connectionStatusLabel.Text = "Connection Failed";
            connectionStatusLabel.ForeColor = Color.Red;
            AddSystemMessage($"❌ Schema refresh failed: {ex.Message}");
            _logger.LogError(ex, "Schema refresh failed");
        }
        finally
        {
            refreshSchemaToolStripMenuItem.Enabled = true;
            queryStatusLabel.Text = "Ready";
            progressBar.Visible = false;
        }
    }

    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var aboutMessage = "DbAutoChat.Win v1.0\n\n" +
                          "Natural Language Database Query Tool\n" +
                          "Supports English and Hinglish queries\n\n" +
                          "Built with .NET 8 and Windows Forms\n" +
                          "© 2024 DbAutoChat";

        MessageBox.Show(aboutMessage, "About DbAutoChat.Win", 
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}