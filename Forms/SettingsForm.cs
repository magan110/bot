using DbAutoChat.Win.Interfaces;
using Microsoft.Extensions.Logging;

namespace DbAutoChat.Win.Forms;

public partial class SettingsForm : Form
{
    private readonly IConfigurationService _configService;

    public SettingsForm(IConfigurationService configService)
    {
        _configService = configService;
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            // Load available databases
            LoadDatabaseOptions();
            
            // Load database connection
            connectionStringTextBox.Text = _configService.GetSetting<string>("ConnectionStrings:Default") ?? "";

            // Load LLM provider settings
            var provider = _configService.GetSetting<string>("Bot:Provider") ?? "OpenAI";
            providerComboBox.SelectedItem = provider;

            // Load API keys
            openAiApiKeyTextBox.Text = _configService.GetSetting<string>("Bot:OpenAI:ApiKey") ?? "";
            geminiApiKeyTextBox.Text = _configService.GetSetting<string>("Bot:Gemini:ApiKey") ?? "";
            ollamaUrlTextBox.Text = _configService.GetSetting<string>("Bot:Ollama:BaseUrl") ?? "http://localhost:11434";

            // Load other settings
            maxRowsNumericUpDown.Value = _configService.GetSetting<int>("Bot:MaxRows");

            UpdateProviderPanels();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}", "Settings Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LoadDatabaseOptions()
    {
        // Add database selector if it exists in the designer
        if (Controls.Find("databaseComboBox", true).FirstOrDefault() is ComboBox dbComboBox)
        {
            dbComboBox.Items.Clear();
            dbComboBox.Items.AddRange(new[] { "Default (Local)", "bwlive", "itkHaria", "imageData" });
            dbComboBox.SelectedIndex = 0;
            dbComboBox.SelectedIndexChanged += DatabaseComboBox_SelectedIndexChanged;
        }
    }

    private void DatabaseComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is ComboBox dbComboBox)
        {
            var selectedDb = dbComboBox.SelectedItem?.ToString();
            var connectionKey = selectedDb switch
            {
                "Default (Local)" => "ConnectionStrings:Default",
                "bwlive" => "ConnectionStrings:bwlive", 
                "itkHaria" => "ConnectionStrings:itkHaria",
                "imageData" => "ConnectionStrings:imageData",
                _ => "ConnectionStrings:Default"
            };
            
            connectionStringTextBox.Text = _configService.GetSetting<string>(connectionKey) ?? "";
        }
    }

    private void UpdateProviderPanels()
    {
        var selectedProvider = providerComboBox.SelectedItem?.ToString() ?? "OpenAI";
        
        openAiPanel.Visible = selectedProvider == "OpenAI";
        geminiPanel.Visible = selectedProvider == "Gemini";
        ollamaPanel.Visible = selectedProvider == "Ollama";
    }

    private void ProviderComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateProviderPanels();
    }

    private void TestConnectionButton_Click(object sender, EventArgs e)
    {
        testConnectionButton.Enabled = false;
        testConnectionButton.Text = "Testing...";

        try
        {
            // Simple connection test
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionStringTextBox.Text);
            connection.Open();
            
            MessageBox.Show("Connection successful!", "Test Connection", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Connection failed: {ex.Message}", "Test Connection", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            testConnectionButton.Enabled = true;
            testConnectionButton.Text = "Test Connection";
        }
    }

    private async void SaveButton_Click(object sender, EventArgs e)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(connectionStringTextBox.Text))
            {
                MessageBox.Show("Please enter a database connection string.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save the current connection string as Default
            _configService.SetSetting("ConnectionStrings:Default", connectionStringTextBox.Text);

            var selectedProvider = providerComboBox.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedProvider))
            {
                MessageBox.Show("Please select an LLM provider.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate API keys based on selected provider
            if (selectedProvider == "OpenAI" && string.IsNullOrWhiteSpace(openAiApiKeyTextBox.Text))
            {
                MessageBox.Show("Please enter an OpenAI API key.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedProvider == "Gemini" && string.IsNullOrWhiteSpace(geminiApiKeyTextBox.Text))
            {
                MessageBox.Show("Please enter a Gemini API key.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedProvider == "Ollama" && string.IsNullOrWhiteSpace(ollamaUrlTextBox.Text))
            {
                MessageBox.Show("Please enter an Ollama URL.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save settings
            _configService.SetSetting("ConnectionStrings:Default", connectionStringTextBox.Text);
            _configService.SetSetting("Bot:Provider", selectedProvider);
            _configService.SetSetting("Bot:MaxRows", (int)maxRowsNumericUpDown.Value);
            _configService.SetSetting("Bot:OpenAI:ApiKey", openAiApiKeyTextBox.Text);
            _configService.SetSetting("Bot:Gemini:ApiKey", geminiApiKeyTextBox.Text);
            _configService.SetSetting("Bot:Ollama:BaseUrl", ollamaUrlTextBox.Text);

            await _configService.SaveAsync();

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Save Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}