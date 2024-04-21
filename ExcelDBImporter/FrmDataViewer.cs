using ExcelDBImporter.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using ExcelDBImporter.Models;
using ExcelDBImporter.Tool;
using System.Diagnostics;


namespace ExcelDBImporter
{
    public partial class FrmDataViewer : Form
    {
        private readonly ExcelDbContext dbContextAlias;
        private class ComboBoxItem(string displayName, object value)
        {
            public string DisplayName { get; set; } = displayName;
            public object Value { get; set; } = value;

            public override string ToString()
            {
                return DisplayName;
            }
        }
        public FrmDataViewer()
        {
            InitializeComponent();
            ComboBoxInitialize();
            dbContextAlias = new();
        }

        private void ComboBoxInitialize() 
        {
            //表示名と実際の入力値のペアのリストを作る
            List<ComboBoxItem> comboBoxItems =
            [
                new ComboBoxItem(GetAllProperty.GetPropertyComment<ShShukka>(nameof(ShShukka) ?? nameof(ShShukka))!
                                ,nameof(ShShukka)),
                new ComboBoxItem(GetAllProperty.GetPropertyComment<ShInOut>(nameof(ShInOut) ?? nameof(ShInOut))!
                                ,nameof(ShInOut)),      
            ];
            CmbBoxSelectClass.DataSource = comboBoxItems;
        }
        /// <summary>
        /// クラス名選択ドロップダウンボックスが選択確定した
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbBoxSelectClass_SelectionChangeCommitted(object sender, EventArgs e) 
        {
            if (CmbBoxSelectClass.SelectedIndex == -1) { return; }
            ComboBoxItem? selectedItem = CmbBoxSelectClass.SelectedItem as ComboBoxItem;
            if (selectedItem == null) { return; }
            string StrSelectedClass = (string)selectedItem!.Value!;
#pragma warning disable IDE0305 // コレクションの初期化を簡略化します
            SortableBindingList<TableDBcolumnNameAndExcelFieldName> blistAlias =new(dbContextAlias.TableDBcolumnNameAndExcelFieldNames
                                                    .Include(ta => ta.Alias)
                                                    .Where(l => l.StrClassName == StrSelectedClass)
                                                    .OrderBy(t => t.StrClassName)
                                                    .ThenBy(t => t.StrDBColumnName)
                                                    .ToList());
#pragma warning restore IDE0305 // コレクションの初期化を簡略化します
            dgvUpdater.AutoGenerateColumns = true;
            dgvUpdater.DataSource = blistAlias;
            dgvUpdater.AutoResizeColumns();
        }
        private void FrmDataViewer_Load(object sender, EventArgs e)
        {
            dgvUpdater.AutoGenerateColumns = false;
            LoadData();

        }
        private void LoadData()
        {
            try
                {
                //Aliasテーブルのキーの存在チェック(ない場合は追加)
                Tool.RegistAllClassAndPropertys RegistClass = new(typeof(ShShukka).Namespace ?? string.Empty,nameof(ShShukka));
                RegistClass.AddAliasNameTableByClassnameAndNamespace(typeof(ShShukka).Namespace ?? string.Empty, nameof(ShShukka));
                //ShInOutテーブルのキーの存在チェック
                RegistAllClassAndPropertys RegistInOut = new(typeof(ShInOut).Name ?? string.Empty,nameof(ShInOut));
                RegistInOut.AddAliasNameTableByClassnameAndNamespace(typeof (ShInOut).Namespace ?? string.Empty,nameof (ShInOut));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
            }
        }

        private void BtnSaveChanges_Click(object sender, EventArgs e)
        {
            MessageBox.Show(dbContextAlias.SaveChanges().ToString() + " 件更新しました");
        }
    }
}
