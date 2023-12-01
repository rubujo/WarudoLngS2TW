// See https://aka.ms/new-console-template for more information.
using Ionic.Zip;
using Ionic.Zlib;
using Newtonsoft.Json;
using OpenCCNET;
using System.Reflection;
using System.Text;

#region 變數宣告

// 隔分符號。
char[] separators = ['｜'];

// 暫存替換用的字詞的字典。
Dictionary<string, string> DictReplacePhrases = [];

#endregion

// 主要程式區塊。
try
{
    Console.Title = $"Warudo 「簡體中文」語系檔案「正體中文」化轉換程式{GetVersion()}";

    WriteLog(value: "此主控台應用程式為將 Warudo「簡體中文」語系檔案「正體中文」化的轉換程式。");
    WriteLog(value: string.Empty);
    WriteLog(value: "※按 Ctrl 鍵 + C 鍵以結束應用程式。");
    WriteLog(value: string.Empty);

    // 設定 OpenCC 字典的替換字詞。
    SetDictionaryReplacePhrases();

    // 設定替換用的字詞。
    SetReplacePhrases();

GetPath:
    WriteLog(value: "請輸入有效的 Warudo 語系檔案的路徑：");

    // 取得使用者輸入的路徑。
    string? path = Console.ReadLine();

    WriteLog(value: string.Empty);

    if (string.IsNullOrEmpty(value: path))
    {
        // 返回 GetPath。
        goto GetPath;
    }
    else
    {
        // 執行翻譯。
        int result = Translate(sourcePath: path, enableDebug: false);

        // 判斷執行結果，當值不為 0 則表示有發生錯誤。
        if (result == 0)
        {
            WriteLog(value: string.Empty);
            WriteLog(value: "請按任意按鍵以結束應用程式……");

            // 取得使用者點擊的按鍵。
            Console.ReadKey();
        }
        else
        {
            // 返回 GetPath。
            goto GetPath;
        }
    }
}
catch (Exception ex)
{
    WriteLog(value: ex.ToString());
}

#region 方法

// 翻譯。
int Translate(string sourcePath, bool enableDebug = false)
{
    try
    {
        if (!Directory.Exists(sourcePath))
        {
            WriteLog(value: $"發生錯誤，資料夾「{sourcePath}」不存在！");
            WriteLog(value: string.Empty);

            return -1;
        }

        // 目標語系字串。
        string targetLang = "zh_CN",
            targetZipFilePath = @$"C:\Users\{Environment.UserName}\Desktop\Warudo_Lang-zh_CN-Backup_{DateTime.Now:yyyyMMddHHmmss}.zip";

        // 篩選出檔案名稱中包含目標語系字串的 JSON 檔案。
        string[] files = Directory.GetFiles(sourcePath).Where(n => Path.GetFileName(n).Contains(targetLang) && Path.GetExtension(n) == ".json").ToArray();

        #region 先用 ZIP 格式的壓縮檔案備份原始檔案

        using ZipFile zipFile = new(targetZipFilePath);

        zipFile.AlternateEncoding = Encoding.UTF8;
        zipFile.AlternateEncodingUsage = ZipOption.AsNecessary;
        zipFile.UseZip64WhenSaving = Zip64Option.AsNecessary;
        zipFile.CompressionLevel = CompressionLevel.BestCompression;
        zipFile.AddFiles(files, string.Empty);
        zipFile.Save();

        #endregion

        WriteLog(value: $"已建立備份用 ZIP 格式的壓縮檔案，檔案路徑：{targetZipFilePath}");
        WriteLog(value: string.Empty);

        // 開始依序處理 JSON 檔案。
        foreach (string file in files)
        {
            StringBuilder stringBuilder = new();

            using JsonWriter jsonWriter = new JsonTextWriter(new StringWriter(sb: stringBuilder));

            // 設定 JSON 內容的格式。
            jsonWriter.Formatting = Formatting.Indented;

            // 讀取檔案的文字內容。
            string textContent = File.ReadAllText(path: file);

            // 載入 JSON 的內容。
            using JsonTextReader jsonTextReader = new(new StringReader(s: textContent));

            #region 開始依序處理 JSON 的內容

            while (jsonTextReader.Read())
            {
                if (enableDebug)
                {
                    if (jsonTextReader.Value == null)
                    {
                        WriteLog(value: $"[Debug] 標誌：{jsonTextReader.TokenType}");
                        WriteLog(value: string.Empty);
                    }
                    else
                    {
                        WriteLog(value: $"[Debug] 標誌：{jsonTextReader.TokenType}，值：{jsonTextReader.Value}");
                        WriteLog(value: string.Empty);
                    }
                }

                // 判斷 jsonTextReader 的 TokenType 並對 jsonWriter 執行相對應的行為。
                switch (jsonTextReader.TokenType)
                {
                    case JsonToken.StartObject:
                        jsonWriter.WriteStartObject();

                        break;
                    case JsonToken.EndObject:
                        jsonWriter.WriteEndObject();

                        break;
                    case JsonToken.StartConstructor:
                        jsonWriter.WriteStartConstructor(name: jsonTextReader.Value?.ToString() ?? string.Empty);

                        break;
                    case JsonToken.EndConstructor:
                        jsonWriter.WriteEndConstructor();

                        break;
                    case JsonToken.StartArray:
                        jsonWriter.WriteStartArray();

                        break;
                    case JsonToken.EndArray:
                        jsonWriter.WriteEndArray();

                        break;
                    case JsonToken.Comment:
                        jsonWriter.WriteComment(GetStringValue(value: jsonTextReader.Value));

                        break;
                    case JsonToken.PropertyName:
                        jsonWriter.WritePropertyName(name: jsonTextReader.Value?.ToString() ?? string.Empty);

                        break;
                    case JsonToken.String:
                        jsonWriter.WriteValue(GetStringValue(value: jsonTextReader.Value));

                        break;
                    case JsonToken.Integer:
                        jsonWriter.WriteValue(Convert.ToInt32(value: jsonTextReader.Value));

                        break;
                    case JsonToken.Float:
                        jsonWriter.WriteValue(Convert.ToSingle(value: jsonTextReader.Value));

                        break;
                    case JsonToken.Boolean:
                        jsonWriter.WriteValue(Convert.ToBoolean(value: jsonTextReader.Value));

                        break;
                    case JsonToken.Date:
                        jsonWriter.WriteValue(Convert.ToDateTime(value: jsonTextReader.Value));

                        break;
                    case JsonToken.Bytes:
                        jsonWriter.WriteValue(Convert.ToByte(value: jsonTextReader.Value));

                        break;
                    case JsonToken.Raw:
                        jsonWriter.WriteRaw(GetStringValue(value: jsonTextReader.Value));

                        break;
                    case JsonToken.Null:
                        jsonWriter.WriteNull();

                        break;
                    case JsonToken.Undefined:
                        jsonWriter.WriteUndefined();

                        break;
                    case JsonToken.None:
                        break;
                    default:
                        break;
                }
            }

            #endregion

            // 覆寫原本的 JSON 檔案。
            using StreamWriter streamWriter = new(path: file, append: false, Encoding.UTF8);

            streamWriter.Write(value: stringBuilder.ToString());

            string fileName = Path.GetFileName(path: file);

            WriteLog(value: $"已將 JSON 檔案「{fileName}」從「簡體中文」轉換成「正體中文」！");
        }

        return 0;
    }
    catch (Exception ex)
    {
        WriteLog(value: $"發生錯誤：{ex}");

        return -1;
    }
}

// 取得字串值。
string GetStringValue(object? value, bool isIdiomConvert = true)
{
    string textValue = Convert.ToString(value) ?? string.Empty;

    if (!string.IsNullOrEmpty(textValue))
    {
        // 從簡體中文轉換成正體中文。
        textValue = ZhConverter.HansToTW(text: textValue, isIdiomConvert: isIdiomConvert);
        // 後處理替換字詞。
        textValue = PostReplacePhrases(value: textValue);
    }

    return textValue;
}

// 後處理替換字詞。
string PostReplacePhrases(string value)
{
    // 當 DictReplacePhrases 有內容時才進行進一步的字詞替換。
    if (DictReplacePhrases.Count <= 0)
    {
        return value;
    }

    // 批次比對字串跟替換詞。
    // ※不是很好的做法，尤其當資料量一大時，可能會有可觀的效能問題。
    for (int i = 0; i < DictReplacePhrases.Count; i++)
    {
        KeyValuePair<string, string> element = DictReplacePhrases.ElementAtOrDefault(i);

        if (value.Contains(element.Key))
        {
            value = value.Replace(element.Key, element.Value);
        }
    }

    return value;
}

// 設定 OpenCC 字典的替換字詞。
void SetDictionaryReplacePhrases()
{
    // 查詢 ReplacePhrases.txt。
    string? file = Directory.GetFiles(AppContext.BaseDirectory)
        .FirstOrDefault(n => Path.GetFileNameWithoutExtension(n) == "ReplaceDictionary" &&
            Path.GetExtension(n) == ".txt");

    if (string.IsNullOrEmpty(file))
    {
        return;
    }

    // 讀取檔案內的每一行。
    string[] lines = File.ReadAllLines(path: file);

    if (lines.Length <= 0)
    {
        return;
    }

    WriteLog(value: "已找到 ReplaceDictionary.txt 檔案，正在設定 OpenCC 字典的替換字詞……");

    int okayCount = 0;

    foreach (string line in lines)
    {
        // 判斷是否為空行或是註解行。
        if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
        {
            continue;
        }

        // 使用分隔分符號拆分每一行的內容。
        // 索引值：0 -> 鍵值，1 -> 替換詞。
        string[] tempArray = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        // 判斷 tempArray 的長度是否等於 2。
        if (tempArray.Length != 2)
        {
            continue;
        }
        else
        {
            // 先移除，再新增，用以避免鍵值已存在的錯誤。
            ZhConverter.ZhDictionary.TWPhrases.Remove(tempArray[0]);
            ZhConverter.ZhDictionary.TWPhrases.Add(tempArray[0], tempArray[1]);

            // 累計成功次數。
            okayCount++;
        }
    }

    if (okayCount > 0)
    {
        WriteLog(value: $"OpenCC 字典的替換字詞設定完成，共已替換 {okayCount} 個字詞。");
    }
    else
    {
        WriteLog(value: "取消設定 OpenCC 字典的替換字詞，因無有效的字詞內容可供替換。");
    }

    WriteLog(value: string.Empty);
}

// 設定替換用的字詞。
void SetReplacePhrases()
{
    // 查詢 ReplacePhrases.txt。
    string? file = Directory.GetFiles(AppContext.BaseDirectory)
        .FirstOrDefault(n => Path.GetFileNameWithoutExtension(n) == "ReplacePhrases" &&
            Path.GetExtension(n) == ".txt");

    if (string.IsNullOrEmpty(file))
    {
        return;
    }

    // 讀取檔案內的每一行。
    string[] lines = File.ReadAllLines(path: file);

    if (lines.Length <= 0)
    {
        return;
    }

    WriteLog(value: "已找到 ReplacePhrases.txt 檔案，正在設定替換用的字詞……");

    int okayCount = 0;

    foreach (string line in lines)
    {
        // 判斷是否為空行或是註解行。
        if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
        {
            continue;
        }

        // 使用分隔分符號拆分每一行的內容。
        // 索引值：0 -> 鍵值，1 -> 替換詞。
        string[] tempArray = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        // 判斷 tempArray 的長度是否等於 2。
        if (tempArray.Length != 2)
        {
            continue;
        }
        else
        {
            // 當鍵值不存在於 DictReplacePhrases 時才新增。
            if (!DictReplacePhrases.ContainsKey(tempArray[0]))
            {
                DictReplacePhrases.Add(tempArray[0], tempArray[1]);
            }

            // 累計成功次數。
            okayCount++;
        }
    }

    if (okayCount > 0)
    {
        WriteLog(value: $"替換用的字詞載入完成，共載入 {okayCount} 個字詞。");
    }
    else
    {
        WriteLog(value: "取消設定替換用的字詞，因無有效的字詞內容可供替換。");
    }

    WriteLog(value: string.Empty);
}

// 寫紀錄。
void WriteLog(string? value, bool withTimestamp = false)
{
    Console.WriteLine($"{(withTimestamp ? $"[{DateTime.Now:yyyy/MM/dd HH:mm:ss} ]" : string.Empty)}{value}");
}

// 取得版本號。
string GetVersion()
{
    Version? version = Assembly.GetExecutingAssembly().GetName().Version;

    if (version != null)
    {
        return $" v{version}";
    }

    return string.Empty;
}

#endregion