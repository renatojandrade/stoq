using Microsoft.EntityFrameworkCore.Migrations;
using Stoq.Catalog.Domain.Constants;

#nullable disable

namespace Stoq.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                INSERT INTO catalog."Categories" ("Id", "NameEn", "NamePt") VALUES
                ('{WellKnownIds.Categories.Uncategorized}',        'Uncategorized',           'Sem categoria'),
                ('{WellKnownIds.Categories.DairyAndEggs}',         'Dairy & Eggs',            'Laticínios e Ovos'),
                ('{WellKnownIds.Categories.MeatAndSeafood}',       'Meat & Seafood',          'Carnes e Frutos do Mar'),
                ('{WellKnownIds.Categories.FruitsAndVegetables}',  'Fruits & Vegetables',     'Frutas e Verduras'),
                ('{WellKnownIds.Categories.BakeryAndBread}',       'Bakery & Bread',          'Padaria e Pães'),
                ('{WellKnownIds.Categories.Beverages}',            'Beverages',               'Bebidas'),
                ('{WellKnownIds.Categories.SnacksAndSweets}',      'Snacks & Sweets',         'Snacks e Doces'),
                ('{WellKnownIds.Categories.FrozenFoods}',          'Frozen Foods',            'Alimentos Congelados'),
                ('{WellKnownIds.Categories.CannedAndPackaged}',    'Canned & Packaged',       'Enlatados e Embalados'),
                ('{WellKnownIds.Categories.CleaningAndHousehold}', 'Cleaning & Household',    'Limpeza e Casa'),
                ('{WellKnownIds.Categories.PersonalCare}',         'Personal Care',           'Higiene Pessoal'),
                ('{WellKnownIds.Categories.PetSupplies}',          'Pet Supplies',            'Produtos para Pets'),
                ('{WellKnownIds.Categories.CondimentsAndSauces}',  'Condiments & Sauces',     'Condimentos e Molhos'),
                ('{WellKnownIds.Categories.SpicesAndSeasonings}',  'Spices & Seasonings',     'Especiarias e Temperos'),
                ('{WellKnownIds.Categories.CerealsAndBreakfast}',  'Cereals & Breakfast',     'Cereais e Café da Manhã'),
                ('{WellKnownIds.Categories.PastaAndRice}',         'Pasta & Rice',            'Massas e Arroz'),
                ('{WellKnownIds.Categories.OilAndVinegar}',        'Oil & Vinegar',           'Óleos e Vinagres'),
                ('{WellKnownIds.Categories.NutsAndDriedFruits}',   'Nuts & Dried Fruits',     'Castanhas e Frutas Secas')
                ON CONFLICT ("Id") DO NOTHING;
                """);

            migrationBuilder.Sql($"""
                INSERT INTO catalog."UnitsOfMeasure" ("Id", "Code", "NameEn", "NamePt") VALUES
                ('{WellKnownIds.Units.UnitsId}',    'units', 'Units',       'Unidades'),
                ('{WellKnownIds.Units.Grams}',      'g',     'Grams',       'Gramas'),
                ('{WellKnownIds.Units.Kilograms}',  'kg',    'Kilograms',   'Quilogramas'),
                ('{WellKnownIds.Units.Millilitres}','ml',    'Millilitres', 'Mililitros'),
                ('{WellKnownIds.Units.Litres}',     'l',     'Litres',      'Litros'),
                ('{WellKnownIds.Units.Milligrams}', 'mg',    'Milligrams',  'Miligramas'),
                ('{WellKnownIds.Units.Pieces}',     'pcs',   'Pieces',      'Peças'),
                ('{WellKnownIds.Units.Boxes}',      'box',   'Boxes',       'Caixas'),
                ('{WellKnownIds.Units.Packs}',      'pack',  'Packs',       'Pacotes'),
                ('{WellKnownIds.Units.Bottles}',    'btl',   'Bottles',     'Garrafas'),
                ('{WellKnownIds.Units.Cans}',       'can',   'Cans',        'Latas'),
                ('{WellKnownIds.Units.Bags}',       'bag',   'Bags',        'Sacos'),
                ('{WellKnownIds.Units.Dozens}',     'dz',    'Dozens',      'Dúzias')
                ON CONFLICT ("Id") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""
                DELETE FROM catalog."UnitsOfMeasure"
                WHERE "Id" IN (
                    '{WellKnownIds.Units.UnitsId}', '{WellKnownIds.Units.Grams}',
                    '{WellKnownIds.Units.Kilograms}', '{WellKnownIds.Units.Millilitres}',
                    '{WellKnownIds.Units.Litres}', '{WellKnownIds.Units.Milligrams}',
                    '{WellKnownIds.Units.Pieces}', '{WellKnownIds.Units.Boxes}',
                    '{WellKnownIds.Units.Packs}', '{WellKnownIds.Units.Bottles}',
                    '{WellKnownIds.Units.Cans}', '{WellKnownIds.Units.Bags}',
                    '{WellKnownIds.Units.Dozens}'
                );
                """);

            migrationBuilder.Sql($"""
                DELETE FROM catalog."Categories"
                WHERE "Id" IN (
                    '{WellKnownIds.Categories.Uncategorized}', '{WellKnownIds.Categories.DairyAndEggs}',
                    '{WellKnownIds.Categories.MeatAndSeafood}', '{WellKnownIds.Categories.FruitsAndVegetables}',
                    '{WellKnownIds.Categories.BakeryAndBread}', '{WellKnownIds.Categories.Beverages}',
                    '{WellKnownIds.Categories.SnacksAndSweets}', '{WellKnownIds.Categories.FrozenFoods}',
                    '{WellKnownIds.Categories.CannedAndPackaged}', '{WellKnownIds.Categories.CleaningAndHousehold}',
                    '{WellKnownIds.Categories.PersonalCare}', '{WellKnownIds.Categories.PetSupplies}',
                    '{WellKnownIds.Categories.CondimentsAndSauces}', '{WellKnownIds.Categories.SpicesAndSeasonings}',
                    '{WellKnownIds.Categories.CerealsAndBreakfast}', '{WellKnownIds.Categories.PastaAndRice}',
                    '{WellKnownIds.Categories.OilAndVinegar}', '{WellKnownIds.Categories.NutsAndDriedFruits}'
                );
                """);
        }
    }
}
