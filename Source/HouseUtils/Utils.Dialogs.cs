using System;
using System.Drawing.Printing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace HouseUtils
{
  public class Dialogs
  {
    static public void AdoptForm(Form form, Control parent)
    {
      form.TopLevel = false;
      form.Parent = parent;
      form.ControlBox = false;
      form.Text = "";
      form.FormBorderStyle = FormBorderStyle.None;
      form.Dock = DockStyle.Fill;
      form.Visible = true;
    }


    static public void ShowError(string message)
    {
      MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }


    static public void ShowInfo(string message)
    {
      MessageBox.Show(message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }


    static public bool Confirm(string message)
    {
      bool result = false;

      if (DialogResult.OK == MessageBox.Show(message, "Confirm", MessageBoxButtons.OKCancel))
      {
        result = true;
      }

      return result;
    }


    static public string SelectFile(string filter = null, string title = null, string initialDirectory = null)
    {
      string result;

      try
      {
        result = ExecuteOpenFileDialog(false, true, filter, title, initialDirectory, null)[0];
      }
      catch
      {
        result = null;
      }

      return result;
    }


    static public string[] SelectMultipleFiles(string filter = null, string title = null, string initialDirectory = null)
    {
      return ExecuteOpenFileDialog(true, true, filter, title, initialDirectory, null);
    }


    static public string SelectWhereToSave(string filter = null, string title = null, string initialDirectory = null, string defaultFilename = null, bool overwritePrompt = true)
    {
      return ExecuteSaveFileDialog(filter, title, initialDirectory, defaultFilename, overwritePrompt);
    }


    static public string SelectFolder(string title, string initialDirectory)
    {
      string result;

      try
      {
        if (String.IsNullOrEmpty(title))
        {
          title = "Select Folder";
        }

        string[] names = ExecuteOpenFileDialog(false, false, "All files (*.*)|*.*", title, initialDirectory, "Folder Selection.");
        result = Path.GetDirectoryName(names[0]);
      }
      catch
      {
        result = null;
      }

      return result;
    }


    static private string[] ExecuteOpenFileDialog(bool multiSelect, bool checkFileExists, string filter, string title, string initialDirectory, string defaultFilename)
    {
      OpenFileDialog dialog = new OpenFileDialog();

      // Configure dialog
      dialog.Multiselect = multiSelect;
      dialog.CheckFileExists = checkFileExists;

      if (String.IsNullOrEmpty(filter) == false)
      {
        dialog.Filter = filter;
      }

      if (String.IsNullOrEmpty(title) == false)
      {
        dialog.Title = title;
      }

      if (String.IsNullOrEmpty(initialDirectory) == false)
      {
        dialog.InitialDirectory = initialDirectory;
      }

      if (String.IsNullOrEmpty(defaultFilename) == false)
      {
        dialog.FileName = defaultFilename;
      }

      // Execute dialog
      string[] result;

      if (dialog.ShowDialog() == DialogResult.OK)
      {
        result = dialog.FileNames;
      }
      else
      {
        result = null;
      }

      return result;
    }


    static private string ExecuteSaveFileDialog(string filter, string title, string initialDirectory, string defaultFilename, bool overwritePrompt)
    {
      SaveFileDialog dialog = new SaveFileDialog();

      // Configure dialog
      if (String.IsNullOrEmpty(filter) == false)
      {
        dialog.Filter = filter;
      }

      if (String.IsNullOrEmpty(title) == false)
      {
        dialog.Title = title;
      }

      if (String.IsNullOrEmpty(initialDirectory) == false)
      {
        dialog.InitialDirectory = initialDirectory;
      }

      if (String.IsNullOrEmpty(defaultFilename) == false)
      {
        dialog.FileName = defaultFilename;
      }

      dialog.AddExtension = false;
      dialog.FilterIndex = 1;
      dialog.OverwritePrompt = overwritePrompt;

      // Execute dialog
      string result = null;

      if (dialog.ShowDialog() == DialogResult.OK)
      {
        result = dialog.FileName;
      }
      else
      {
        result = null;
      }

      return result;
    }


    static public string ExecuteFolderDialog(string startPath, string description)
    {
      string result = "";

      FolderBrowserDialog dialog = new FolderBrowserDialog();

      dialog.SelectedPath = startPath;
      dialog.Description = description;

      if (dialog.ShowDialog() == DialogResult.OK)
      {
        result = dialog.SelectedPath;
      }

      return result;
    }


    static public PrinterSettings ExecutePrintDialog(string printerName, int minPage, int maxPage)
    {
      PrinterSettings result = null;

      PrintDialog dlg = new PrintDialog();
      dlg.AllowSomePages = true;
      dlg.UseEXDialog = true;
      dlg.PrinterSettings.PrinterName = printerName;
      dlg.PrinterSettings.MinimumPage = minPage;
      dlg.PrinterSettings.MaximumPage = maxPage;
      dlg.PrinterSettings.FromPage = dlg.PrinterSettings.MinimumPage;
      dlg.PrinterSettings.ToPage = dlg.PrinterSettings.MaximumPage;

      if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        result = dlg.PrinterSettings;
      }

      return result;
    }


    static public void ExecutePrintPreview(PrintDocument printDocument)
    {
      PrintPreviewDialog dlg = new PrintPreviewDialog();
      dlg.Document = printDocument;
      dlg.ShowDialog();
    }
  }
}
