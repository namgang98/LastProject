using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// CSV 문자열 → 표(헤더 + 행) 로 바꿔주는 클래스 (유니티와 무관한 순수 C#)
// - 1행은 열 이름(헤더). 열은 "이름"으로 찾기 때문에 시트에서 열 순서를 바꿔도 된다
// - "값, 쉼표 포함" 처럼 따옴표로 감싼 칸, 따옴표 안의 "" (따옴표 자체) 도 처리
// - 빈 줄, 첫 칸이 # 으로 시작하는 줄(메모용)은 건너뛴다
public class CsvTable
{
    private readonly Dictionary<string, int> columns = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string[]> rows = new();
    private readonly List<int> lineNumbers = new();

    public string Name { get; }
    public int RowCount => rows.Count;

    private CsvTable(string name)
    {
        Name = name;
    }

    public static CsvTable Parse(string name, string text)
    {
        var table = new CsvTable(name);

        if(string.IsNullOrEmpty(text))
            return table;

        List<string[]> records = SplitRecords(text.TrimStart('﻿'));

        if(records.Count == 0)
            return table;

        string[] header = records[0];

        for(int i = 0; i < header.Length; i++)
        {
            string column = header[i].Trim();

            if(column.Length > 0 && !table.columns.ContainsKey(column))
                table.columns.Add(column, i);
        }

        for(int i = 1; i < records.Count; i++)
        {
            string[] record = records[i];

            if(record.Length > 0 && record[0].TrimStart().StartsWith("#"))
                continue;

            table.rows.Add(record);
            table.lineNumbers.Add(i + 1);   // 시트의 실제 행 번호 (헤더가 1행)
        }

        return table;
    }

    public bool HasColumn(string column)
    {
        return columns.ContainsKey(column);
    }

    public CsvRow GetRow(int index)
    {
        return new CsvRow(this, rows[index], lineNumbers[index]);
    }

    internal string GetCell(string[] cells, string column)
    {
        if(!columns.TryGetValue(column, out int index))
            return string.Empty;

        if(index >= cells.Length)
            return string.Empty;

        return cells[index].Trim();
    }

    // 문자 하나씩 읽으면서 "따옴표 안인지"를 기억하는 방식 (상태 머신)
    private static List<string[]> SplitRecords(string text)
    {
        var records = new List<string[]>();
        var fields = new List<string>();
        var cell = new StringBuilder();
        bool inQuotes = false;

        for(int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if(inQuotes)
            {
                if(c != '"')
                {
                    cell.Append(c);
                    continue;
                }

                bool escaped = i + 1 < text.Length && text[i + 1] == '"';

                if(escaped)
                {
                    cell.Append('"');
                    i++;
                    continue;
                }

                inQuotes = false;
                continue;
            }

            switch(c)
            {
                case '"':
                    inQuotes = true;
                    break;
                case ',':
                    fields.Add(cell.ToString());
                    cell.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    fields.Add(cell.ToString());
                    cell.Clear();
                    AddRecord(records, fields);
                    break;
                default:
                    cell.Append(c);
                    break;
            }
        }

        if(cell.Length > 0 || fields.Count > 0)
        {
            fields.Add(cell.ToString());
            AddRecord(records, fields);
        }

        return records;
    }

    private static void AddRecord(List<string[]> records, List<string> fields)
    {
        bool empty = true;

        foreach(var field in fields)
        {
            if(field.Trim().Length > 0)
            {
                empty = false;
                break;
            }
        }

        if(!empty)
            records.Add(fields.ToArray());

        fields.Clear();
    }
}

// 표의 한 행. 열 이름으로 값을 꺼내고, 숫자/enum 변환은 TryGet 으로 실패 여부를 돌려준다
public readonly struct CsvRow
{
    private readonly CsvTable table;
    private readonly string[] cells;

    public int LineNumber { get; }

    // 에러 메시지용 위치 : "Weapons 5행"
    public string Where => $"{table.Name} {LineNumber}행";

    public CsvRow(CsvTable table, string[] cells, int lineNumber)
    {
        this.table = table;
        this.cells = cells;
        LineNumber = lineNumber;
    }

    public string GetString(string column)
    {
        return table.GetCell(cells, column);
    }

    public bool TryGetInt(string column, out int value)
    {
        return int.TryParse(GetString(column), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    public bool TryGetFloat(string column, out float value)
    {
        return float.TryParse(GetString(column), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    public bool TryGetEnum<T>(string column, out T value) where T : struct
    {
        return Enum.TryParse(GetString(column), true, out value);
    }
}
