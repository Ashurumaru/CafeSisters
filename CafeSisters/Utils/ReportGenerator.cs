using System;
using System.Collections.Generic;
using System.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;
using CafeSisters.Data;
using System.Windows.Controls;
using System.Data.Entity;
using System.Drawing;
using Microsoft.Win32;
using System.Diagnostics;

namespace CafeSisters.Utils
{
    public static class ReportGenerator
    {
        public static void GenerateOrderReport(int orderId)
        {

            string filePath = $"OrderReport_{orderId}.docx";

            using (var doc = DocX.Create(filePath))
            {
                var context = new CafeSistersEntities();
                var order = context.Orders.Include("OrderDetails.Recipes").FirstOrDefault(o => o.OrderId == orderId);

                if (order == null)
                {
                    throw new Exception("Order not found.");
                }

                var title = doc.InsertParagraph("ООО \"Cafe Sisters\"")
                    .FontSize(14)
                    .Bold()
                    .Alignment = Alignment.center;
                doc.InsertParagraph("Кафе")
                    .FontSize(12)
                    .Alignment = Alignment.center;
                doc.InsertParagraph("\n");
                doc.InsertParagraph($"ЗАКАЗ № {order.OrderId} от {order.OrderDate:dd.MM.yyyy}")
                    .FontSize(12)
                    .Alignment = Alignment.center;
                doc.InsertParagraph("НА ИЗГОТОВЛЕНИЕ ПРОДУКЦИИ")
                    .FontSize(12)
                    .Bold()
                    .Alignment = Alignment.center;
                doc.InsertParagraph("\n");

                var table = doc.AddTable(order.OrderDetails.Count + 2, 6);
                table.Design = TableDesign.TableGrid;

                table.Rows[0].Cells[0].Paragraphs.First().Append("№").Bold();
                table.Rows[0].Cells[1].Paragraphs.First().Append("Наименование").Bold();
                table.Rows[0].Cells[2].Paragraphs.First().Append("Ед.").Bold();
                table.Rows[0].Cells[3].Paragraphs.First().Append("Кол-во").Bold();
                table.Rows[0].Cells[4].Paragraphs.First().Append("Цена").Bold();
                table.Rows[0].Cells[5].Paragraphs.First().Append("Сумма").Bold();

                decimal totalQuantity = 0;
                decimal totalAmount = 0;

                for (int i = 0; i < order.OrderDetails.Count; i++)
                {
                    var detail = order.OrderDetails.ToList()[i];
                    var recipe = detail.Recipes;

                    table.Rows[i + 1].Cells[0].Paragraphs.First().Append((i + 1).ToString());
                    table.Rows[i + 1].Cells[1].Paragraphs.First().Append(recipe.RecipeName);
                    table.Rows[i + 1].Cells[2].Paragraphs.First().Append("шт");
                    table.Rows[i + 1].Cells[3].Paragraphs.First().Append(detail.Quantity.ToString());
                    table.Rows[i + 1].Cells[4].Paragraphs.First().Append(recipe.Cost.ToString("F2"));
                    table.Rows[i + 1].Cells[5].Paragraphs.First().Append((detail.Quantity * recipe.Cost).ToString("F2"));

                    totalQuantity += detail.Quantity;
                    totalAmount += detail.Quantity * recipe.Cost;
                }

                table.Rows[order.OrderDetails.Count + 1].Cells[1].Paragraphs.First().Append("Итого").Bold();
                table.Rows[order.OrderDetails.Count + 1].Cells[3].Paragraphs.First().Append(totalQuantity.ToString()).Bold();
                table.Rows[order.OrderDetails.Count + 1].Cells[5].Paragraphs.First().Append(totalAmount.ToString("F2")).Bold();

                table.SetWidths(new float[] { 30, 200, 30, 40, 60, 80 });
                doc.InsertTable(table);

                doc.Save();
            }
            Process.Start("explorer.exe", filePath);
        }

        public static void GenerateAllOrdersReport(DateTime? startDate, DateTime? endDate, int? employeeId)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Сохранить отчет по всем заказам",
                FileName = $"AllOrdersReport_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.docx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var filePath = saveFileDialog.FileName;
            using (var doc = DocX.Create(filePath))
            {
                var title = doc.InsertParagraph("Отчет о оказанных услугах")
                                   .FontSize(16)
                                   .Bold()
                                   .Alignment = Alignment.center;
                doc.InsertParagraph("\n");

                var periodInfo = doc.InsertParagraph($"Отчетный период: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}")
                   .FontSize(12)
                   .Alignment = Alignment.both;
                doc.InsertParagraph("\n");

                using (var context = new CafeSistersEntities())
                {
                    var query = context.Orders
                        .Include("OrderDetails")
                        .Include("OrderDetails.Recipes")
                        .AsQueryable();

                    if (startDate.HasValue)
                    {
                        query = query.Where(o => o.OrderDate >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(o => o.OrderDate <= endDate.Value);
                    }

                    if (employeeId.HasValue)
                    {
                        query = query.Where(o => o.EmployeeId == employeeId.Value);
                    }

                    var orders = query.ToList();

                    var totalRevenue = orders.Sum(o => o.TotalCost);

                    if (orders.Any())
                    {
                        var table = doc.AddTable(orders.Count + 1, 5);
                        table.Rows[0].Cells[0].Paragraphs.First().Append("Номер заказа").Bold();
                        table.Rows[0].Cells[1].Paragraphs.First().Append("Дата заказа").Bold();
                        table.Rows[0].Cells[2].Paragraphs.First().Append("Сотрудник").Bold();
                        table.Rows[0].Cells[3].Paragraphs.First().Append("Сумма заказа").Bold();
                        table.Rows[0].Cells[4].Paragraphs.First().Append("Детали заказа").Bold();

                        for (int i = 0; i < orders.Count; i++)
                        {
                            var order = orders[i];
                            var employee = context.Employees.FirstOrDefault(e => e.EmployeeId == order.EmployeeId);
                            var employeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Неизвестно";

                            table.Rows[i + 1].Cells[0].Paragraphs.First().Append(order.OrderId.ToString());
                            table.Rows[i + 1].Cells[1].Paragraphs.First().Append(order.OrderDate.ToString());
                            table.Rows[i + 1].Cells[2].Paragraphs.First().Append(employeeName);
                            table.Rows[i + 1].Cells[3].Paragraphs.First().Append($"{order.TotalCost:F2} руб.");

                            var details = string.Join(", ", order.OrderDetails.Select(od => $"{od.Recipes.RecipeName} - {od.Quantity} шт."));
                            table.Rows[i + 1].Cells[4].Paragraphs.First().Append(details);
                        }

                        doc.InsertTable(table);
                        table.Design = TableDesign.TableGrid;
                        table.Alignment = Alignment.center;
                        table.AutoFit = AutoFit.Contents;

                        doc.InsertParagraph($"Общая выручка за указанный период: {totalRevenue:F2} руб.")
                            .FontSize(12)
                            .SpacingAfter(10)
                            .Bold();
                    }
                    else
                    {
                        doc.InsertParagraph("За выбранный период заказы отсутствуют.")
                            .FontSize(12);
                    }
                }

                var signatureTable = doc.AddTable(5, 4);
                string currentDate = DateTime.Now.ToString("«dd» MMMM yyyy г.");

                signatureTable.Rows[1].Cells[0].Paragraphs.First().Append("_________________________");
                signatureTable.Rows[1].Cells[1].Paragraphs.First().Append("_______________");
                signatureTable.Rows[1].Cells[2].Paragraphs.First().Append("__________________________");

                signatureTable.Rows[2].Cells[0].Paragraphs.First().Append("(должность)").Italic().FontSize(10);
                signatureTable.Rows[2].Cells[1].Paragraphs.First().Append("(личная подпись)").Italic().FontSize(10);
                signatureTable.Rows[2].Cells[2].Paragraphs.First().Append("(расшифровка подписи)").Italic().FontSize(10);

                signatureTable.Rows[3].Cells[0].Paragraphs.First().Append("_________________________");
                signatureTable.Rows[3].Cells[1].Paragraphs.First().Append("_______________");
                signatureTable.Rows[3].Cells[2].Paragraphs.First().Append("__________________________");

                signatureTable.Rows[4].Cells[0].Paragraphs.First().Append("(должность)").Italic().FontSize(10);
                signatureTable.Rows[4].Cells[1].Paragraphs.First().Append("(личная подпись)").Italic().FontSize(10);
                signatureTable.Rows[4].Cells[2].Paragraphs.First().Append("(расшифровка подписи)").Italic().FontSize(10);

                signatureTable.SetBorder(TableBorderType.InsideH, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.InsideV, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.Bottom, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.Top, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.Left, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));
                signatureTable.SetBorder(TableBorderType.Right, new Xceed.Document.NET.Border(BorderStyle.Tcbs_none, 0, 0, System.Drawing.Color.White));

                signatureTable.SetColumnWidth(0, 200);
                signatureTable.SetColumnWidth(1, 100);
                signatureTable.SetColumnWidth(2, 220);

                doc.InsertTable(signatureTable);

                doc.Save();
            }
        }


        public static void GenerateMenuReport(int menuId)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Сохранить отчет по меню",
                FileName = $"MenuReport_{menuId}.docx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var filePath = saveFileDialog.FileName;

            using (var context = new CafeSistersEntities())
            {
                var menu = context.Menus
                    .Include(m => m.MenuRecipes.Select(mr => mr.Recipes.RecipeIngredients.Select(ri => ri.Ingredients.NutritionalValues)))
                    .FirstOrDefault(m => m.MenuId == menuId);

                if (menu == null)
                {
                    throw new Exception("Menu not found.");
                }

                using (var doc = DocX.Create(filePath))
                {
                    doc.InsertParagraph($"Отчет по меню: {menu.MenuName}")
                        .FontSize(20)
                        .Bold()
                        .Alignment = Alignment.center;

                    foreach (var menuRecipe in menu.MenuRecipes)
                    {
                        var recipe = menuRecipe.Recipes;

                        doc.InsertParagraph($"{recipe.RecipeName}")
                            .FontSize(16)
                            .Bold()
                            .SpacingAfter(5);

                        doc.InsertParagraph($"Цена: {recipe.Cost:F2} руб.")
                            .FontSize(14)
                            .SpacingAfter(2);

                        doc.InsertParagraph($"Инструкция: {recipe.Instruction}")
                            .FontSize(12)
                            .SpacingAfter(10);

                        var totalProteins = recipe.RecipeIngredients.Sum(ri => (ri.Ingredients.NutritionalValues.FirstOrDefault()?.Proteins ?? 0) * ri.Quantity);
                        var totalFats = recipe.RecipeIngredients.Sum(ri => (ri.Ingredients.NutritionalValues.FirstOrDefault()?.Fats ?? 0) * ri.Quantity);
                        var totalCarbohydrates = recipe.RecipeIngredients.Sum(ri => (ri.Ingredients.NutritionalValues.FirstOrDefault()?.Carbohydrates ?? 0) * ri.Quantity);
                        var totalEnergyValue = recipe.RecipeIngredients.Sum(ri => (ri.Ingredients.NutritionalValues.FirstOrDefault()?.EnergyValue ?? 0) * ri.Quantity);

                        doc.InsertParagraph($"Пищевая ценность на порцию:")
                            .FontSize(12)
                            .SpacingAfter(2);

                        var nutritionTable = doc.AddTable(4, 2);
                        nutritionTable.Design = TableDesign.TableGrid;
                        nutritionTable.Rows[0].Cells[0].Paragraphs.First().Append("Белки").Bold();
                        nutritionTable.Rows[0].Cells[1].Paragraphs.First().Append($"{totalProteins:F2} г");
                        nutritionTable.Rows[1].Cells[0].Paragraphs.First().Append("Жиры").Bold();
                        nutritionTable.Rows[1].Cells[1].Paragraphs.First().Append($"{totalFats:F2} г");
                        nutritionTable.Rows[2].Cells[0].Paragraphs.First().Append("Углеводы").Bold();
                        nutritionTable.Rows[2].Cells[1].Paragraphs.First().Append($"{totalCarbohydrates:F2} г");
                        nutritionTable.Rows[3].Cells[0].Paragraphs.First().Append("Энергетическая ценность").Bold();
                        nutritionTable.Rows[3].Cells[1].Paragraphs.First().Append($"{totalEnergyValue:F2} ккал");
                        doc.InsertTable(nutritionTable);

                        doc.InsertParagraph().SpacingAfter(10);
                    }

                    doc.Save();
                }
            }

            Process.Start("explorer.exe", filePath);
        }



        public static void GenerateAllDishesReport()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Сохранить отчет по всем блюдам",
                FileName = "AllDishesReport.docx"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            var filePath = saveFileDialog.FileName;

            using (var context = new CafeSistersEntities())
            {
                var recipes = context.Recipes
                    .Include(r => r.RecipeIngredients.Select(ri => ri.Ingredients.NutritionalValues))
                    .ToList();

                using (var doc = DocX.Create(filePath))
                {
                    doc.InsertParagraph("Отчет по всем блюдам")
                        .FontSize(20)
                        .Bold()
                        .Alignment = Alignment.center;

                    foreach (var recipe in recipes)
                    {
                        doc.InsertParagraph($"{recipe.RecipeName}")
                            .FontSize(16)
                            .Bold()
                            .SpacingAfter(5);

                        doc.InsertParagraph($"Цена: {recipe.Cost:F2} руб.")
                            .FontSize(14)
                            .SpacingAfter(2);

                        doc.InsertParagraph($"Инструкция: {recipe.Instruction}")
                            .FontSize(12)
                            .SpacingAfter(10);

                        var totalProteins = recipe.RecipeIngredients.Sum(ri => (ri.Ingredients.NutritionalValues.FirstOrDefault()?.Proteins ?? 0) * ri.Quantity);
                        var totalFats = recipe.RecipeIngredients.Sum(ri => (ri.Ingredients.NutritionalValues.FirstOrDefault()?.Fats ?? 0) * ri.Quantity);
                        var totalCarbohydrates = recipe.RecipeIngredients.Sum(ri => (ri.Ingredients.NutritionalValues.FirstOrDefault()?.Carbohydrates ?? 0) * ri.Quantity);
                        var totalEnergyValue = recipe.RecipeIngredients.Sum(ri => (ri.Ingredients.NutritionalValues.FirstOrDefault()?.EnergyValue ?? 0) * ri.Quantity);

                        doc.InsertParagraph($"Пищевая ценность на порцию:")
                            .FontSize(12)
                            .SpacingAfter(2);

                        var nutritionTable = doc.AddTable(4, 2);
                        nutritionTable.Design = TableDesign.TableGrid;
                        nutritionTable.Rows[0].Cells[0].Paragraphs.First().Append("Белки").Bold();
                        nutritionTable.Rows[0].Cells[1].Paragraphs.First().Append($"{totalProteins:F2} г");
                        nutritionTable.Rows[1].Cells[0].Paragraphs.First().Append("Жиры").Bold();
                        nutritionTable.Rows[1].Cells[1].Paragraphs.First().Append($"{totalFats:F2} г");
                        nutritionTable.Rows[2].Cells[0].Paragraphs.First().Append("Углеводы").Bold();
                        nutritionTable.Rows[2].Cells[1].Paragraphs.First().Append($"{totalCarbohydrates:F2} г");
                        nutritionTable.Rows[3].Cells[0].Paragraphs.First().Append("Энергетическая ценность").Bold();
                        nutritionTable.Rows[3].Cells[1].Paragraphs.First().Append($"{totalEnergyValue:F2} ккал");
                        doc.InsertTable(nutritionTable);
                        doc.InsertParagraph("\n");

                        doc.InsertParagraph("Ингредиенты:")
                            .FontSize(14)
                            .SpacingAfter(5);

                        var ingredientsTable = doc.AddTable(recipe.RecipeIngredients.Count + 1, 5);
                        ingredientsTable.Design = TableDesign.TableGrid;

                        ingredientsTable.Rows[0].Cells[0].Paragraphs.First().Append("№").Bold();
                        ingredientsTable.Rows[0].Cells[1].Paragraphs.First().Append("Наименование").Bold();
                        ingredientsTable.Rows[0].Cells[2].Paragraphs.First().Append("Кол-во").Bold();
                        ingredientsTable.Rows[0].Cells[3].Paragraphs.First().Append("Ед.").Bold();
                        ingredientsTable.Rows[0].Cells[4].Paragraphs.First().Append("Категория").Bold();

                        for (int i = 0; i < recipe.RecipeIngredients.Count; i++)
                        {
                            var ingredient = recipe.RecipeIngredients.ToList()[i].Ingredients;

                            ingredientsTable.Rows[i + 1].Cells[0].Paragraphs.First().Append((i + 1).ToString());
                            ingredientsTable.Rows[i + 1].Cells[1].Paragraphs.First().Append(ingredient.IngredientName);
                            ingredientsTable.Rows[i + 1].Cells[2].Paragraphs.First().Append(recipe.RecipeIngredients.ToList()[i].Quantity.ToString());
                            ingredientsTable.Rows[i + 1].Cells[3].Paragraphs.First().Append(ingredient.Unit);
                            ingredientsTable.Rows[i + 1].Cells[4].Paragraphs.First().Append(ingredient.IngredientCategories.CategoryName);
                        }

                        doc.InsertTable(ingredientsTable);
                        doc.InsertParagraph().SpacingAfter(10);
                    }

                    doc.Save();
                }
            }

            Process.Start("explorer.exe", filePath);
        }
    }
}
