﻿// <auto-generated />
using System;
using ExcelDBImporter.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ExcelDBImporter.Migrations
{
    [DbContext(typeof(ExcelDbContext))]
    [Migration("20240406103815_QRinputTable_Add")]
    partial class QRinputTable_Add
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0-preview.1.24081.2");

            modelBuilder.Entity("ExcelDBImporter.Models.AppSetting", b =>
                {
                    b.Property<int>("AppSettingID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("AppSettingID")
                        .HasColumnOrder(0);

                    b.Property<string>("StrAppName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("StrAppName")
                        .HasColumnOrder(1)
                        .HasComment("アプリ名、必須");

                    b.Property<string>("StrLastLoadFromDir")
                        .HasColumnType("TEXT")
                        .HasColumnName("StrLastLoadFromDir")
                        .HasColumnOrder(2)
                        .HasComment("最終読み取りディレクトリ");

                    b.Property<string>("StrLastSaveToDir")
                        .HasColumnType("TEXT")
                        .HasColumnName("StrLastSaveToDir")
                        .HasColumnOrder(3)
                        .HasComment("最終保存ディレクトリ");

                    b.HasKey("AppSettingID");

                    b.ToTable("AppSetting", t =>
                        {
                            t.HasComment("各アプリの設定を格納する。1アプリ１レコード");
                        });
                });

            modelBuilder.Entity("ExcelDBImporter.Models.ShShukka", b =>
                {
                    b.Property<int>("ShShukkaID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(0);

                    b.Property<DateTime?>("DateAssemble")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(9)
                        .HasComment("組立日");

                    b.Property<DateTime?>("DateFunctionTest")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(10)
                        .HasComment("試験日");

                    b.Property<DateTime?>("DateMarshalling")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(8)
                        .HasComment("マーシャリング日");

                    b.Property<DateTime?>("DatePrepare")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(11)
                        .HasComment("出荷準備");

                    b.Property<DateTime?>("DateShippingTest")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(12)
                        .HasComment("出荷検査日");

                    b.Property<DateTime>("DateShukka")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(1)
                        .HasComment("出荷計画");

                    b.Property<DateTime?>("DateShutuzu")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(7)
                        .HasComment("出図日");

                    b.Property<int>("IntAmount")
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(6)
                        .HasComment("発番数量");

                    b.Property<bool>("IsAlreadyOutput")
                        .HasColumnType("INTEGER")
                        .HasColumnOrder(13)
                        .HasComment("データ出力済みフラグ");

                    b.Property<string>("StrHinmei")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(5)
                        .HasComment("品名");

                    b.Property<string>("StrKishu")
                        .HasColumnType("TEXT")
                        .HasColumnOrder(2)
                        .HasComment("機種");

                    b.Property<string>("StrOrderFrom")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(4)
                        .HasComment("注文主");

                    b.Property<string>("StrSeiban")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnOrder(3)
                        .HasComment("製番");

                    b.HasKey("ShShukkaID");

                    b.ToTable("ShShukka", t =>
                        {
                            t.HasComment("発番出荷物件予定表モデルクラス");
                        });
                });

            modelBuilder.Entity("ExcelDBImporter.Models.TQRinput", b =>
                {
                    b.Property<int>("TQRinputId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("TQRinputId")
                        .HasComment("QRテーブルのキー");

                    b.Property<DateTime?>("DateInputDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("DateInputDate")
                        .HasComment("入力日時、作業開始日時として使用");

                    b.Property<DateTime?>("DateToDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("DateToDate")
                        .HasComment("終了日時、基本的に自動計算で入力する");

                    b.Property<uint>("QROPcode")
                        .HasColumnType("INTEGER")
                        .HasColumnName("QROPcode")
                        .HasComment("行程種別のenum。初期値 None");

                    b.HasKey("TQRinputId");

                    b.ToTable("TQRinput", t =>
                        {
                            t.HasComment("工程管理用 QRコード記録テーブル");
                        });
                });

            modelBuilder.Entity("ExcelDBImporter.Models.TableDBcolumnNameAndExcelFieldName", b =>
                {
                    b.Property<int>("TableDBcolumnNameAndExcelFieldNameID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("TableDBcolumnNameAndExcelFieldNameID")
                        .HasColumnOrder(0);

                    b.Property<string>("StrClassName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("StrClassName")
                        .HasColumnOrder(1)
                        .HasComment("格納されているモデルクラス名");

                    b.Property<string>("StrDBColumnName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("StrDBColumnName")
                        .HasColumnOrder(2)
                        .HasComment("モデルクラスのオリジナルDBColumn名");

                    b.Property<string>("StrshProcessManagementFieldName")
                        .HasColumnType("TEXT")
                        .HasColumnName("StrshProcessManagementFieldName")
                        .HasColumnOrder(4)
                        .HasComment("工程管理システムからの出力Excelファイルのフィールド名");

                    b.Property<string>("StrshShukkaFieldName")
                        .HasColumnType("TEXT")
                        .HasColumnName("StrshShukkaFieldName")
                        .HasColumnOrder(3)
                        .HasComment("出荷物件予定表Excelファイルのフィールド名、4行目と5行目の文字列を連結する");

                    b.HasKey("TableDBcolumnNameAndExcelFieldNameID");

                    b.ToTable("TableDBcolumnNameAndExcelFieldName", t =>
                        {
                            t.HasComment("DBとExcelファイルのフィールド名の対応格納テーブル。対応Excelファイルが増えると列が増えていく");
                        });
                });

            modelBuilder.Entity("ExcelDBImporter.Models.TableFieldAliasNameList", b =>
                {
                    b.Property<int>("TableFieldAliasNameListId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("TableFieldAliasNameListId")
                        .HasColumnOrder(0);

                    b.Property<string>("StrColnmnAliasName")
                        .HasColumnType("TEXT")
                        .HasColumnName("StrColnmnAliasName")
                        .HasColumnOrder(2)
                        .HasComment("列の表示用別名");

                    b.Property<int>("TableDBcolumnNameAndExcelFieldNameID")
                        .HasColumnType("INTEGER")
                        .HasColumnName("TableDBcolumnNameAndExcelFieldNameID")
                        .HasColumnOrder(1)
                        .HasComment("DB columnnameテーブルの外部キー");

                    b.HasKey("TableFieldAliasNameListId");

                    b.HasIndex("TableDBcolumnNameAndExcelFieldNameID")
                        .IsUnique();

                    b.ToTable("TableFieldAliasNameList", t =>
                        {
                            t.HasComment("テーブル列名の別名(表示名等)格納テーブル");
                        });
                });

            modelBuilder.Entity("ExcelDBImporter.Models.TableFieldAliasNameList", b =>
                {
                    b.HasOne("ExcelDBImporter.Models.TableDBcolumnNameAndExcelFieldName", "DBcolumn")
                        .WithOne("Alias")
                        .HasForeignKey("ExcelDBImporter.Models.TableFieldAliasNameList", "TableDBcolumnNameAndExcelFieldNameID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DBcolumn");
                });

            modelBuilder.Entity("ExcelDBImporter.Models.TableDBcolumnNameAndExcelFieldName", b =>
                {
                    b.Navigation("Alias");
                });
#pragma warning restore 612, 618
        }
    }
}
