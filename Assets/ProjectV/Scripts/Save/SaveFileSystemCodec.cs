using System; // 예외와 변환 기능
using System.IO; // 파일 기능
using System.Security.Cryptography; // 검사값 계산
using System.Text; // 문자 인코딩
using UnityEngine; // Unity 기본 기능

public static partial class SaveFileSystem // 저장 파일의 내용 만들기와 풀기
{
    private const string ChecksumSalt = "ProjectV.Save.Check"; // 검사값에 섞는 문구

    private static readonly byte[] MaskKey =
        Encoding.UTF8.GetBytes("ProjectV-Grimoire"); // 진행 데이터를 가리는 열쇠

    private static string Encode(SaveData data) // 진행 데이터를 저장 파일 내용으로 만든다.
    {
        string payload = Convert.ToBase64String(
            Mask(Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)))
        );

        SaveFileEnvelope envelope = new SaveFileEnvelope
        {
            format = SaveRules.FormatName,
            version = data.version,
            checksum = ComputeChecksum(payload),
            payload = payload
        };

        return JsonUtility.ToJson(envelope, true);
    }

    // 파일 하나를 읽어 진행 데이터로 푼다. 형식, 검사값, 버전 가운데 하나라도 맞지 않으면 실패한다.
    private static bool TryReadFile(string path, out SaveData data, out string error)
    {
        data = null;

        try
        {
            SaveFileEnvelope envelope =
                JsonUtility.FromJson<SaveFileEnvelope>(File.ReadAllText(path, Encoding.UTF8));

            if (envelope == null ||
                envelope.format != SaveRules.FormatName ||
                string.IsNullOrEmpty(envelope.payload))
            {
                error = "저장 파일 형식이 올바르지 않습니다.";
                return false;
            }

            if (envelope.checksum != ComputeChecksum(envelope.payload))
            {
                error = "저장 파일이 손상되었습니다. (검사값 불일치)";
                return false;
            }

            SaveData readData = JsonUtility.FromJson<SaveData>(
                Encoding.UTF8.GetString(Mask(Convert.FromBase64String(envelope.payload)))
            );

            if (!SaveRules.Upgrade(readData, out error)) { return false; } // 예전 버전이면 지금 구조로 변환

            data = readData;
            return true;
        }
        catch (Exception exception)
        {
            error = "저장 파일을 읽지 못했습니다. " + exception.Message;
            return false;
        }
    }

    private static byte[] Mask(byte[] bytes) // 열쇠와 XOR. 두 번 하면 원래 값으로 돌아온다.
    {
        byte[] masked = new byte[bytes.Length];

        for (int i = 0; i < bytes.Length; i++)
        {
            masked[i] = (byte)(bytes[i] ^ MaskKey[i % MaskKey.Length]);
        }

        return masked;
    }

    private static string ComputeChecksum(string payload) // SHA-256 검사값 (16진수 문자열)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ChecksumSalt + payload));
            StringBuilder builder = new StringBuilder(hash.Length * 2);

            foreach (byte value in hash)
            {
                builder.Append(value.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
