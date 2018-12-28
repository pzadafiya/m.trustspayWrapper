
using NLFSG.BLL.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace BLL.Helper
{
    public class TrustPayPaymentWrapper
    {
        private static String _API_URL = null;
        private static String _SECRET_KEY = null;
        private static Dictionary<string, string> _PARAMS = null;

        public TrustPayPaymentWrapper(String secretKey, Dictionary<string, string> param)
        {
            //_API_URL = "https://shoppingingstore.com/TestTPInterface"; for Testing
            _API_URL = "https://shoppingingstore.com/TPInterface";
            _SECRET_KEY = secretKey;// getKey(secretKey);
            _PARAMS = param;
            validatePayload();
        }

        public respon payment()
        {
            _PARAMS.Remove("ccn");

            #region Generate sha256 Code : signInfo
            String signature = "";
            signature = (String)_PARAMS["merNo"] + (String)_PARAMS["gatewayNo"] + (String)_PARAMS["orderNo"] +
                        (String)_PARAMS["orderCurrency"] +(String)_PARAMS["orderAmount"]+ (String)_PARAMS["firstName"]  +
                        (String)_PARAMS["lastName"] + (String)_PARAMS["cardNo"] + (String)_PARAMS["cardExpireYear"] +
                        (String)_PARAMS["cardExpireMonth"] + (String)_PARAMS["cardSecurityCode"] + (String)_PARAMS["email"] + 
                        _SECRET_KEY;

            if (_PARAMS.ContainsKey("signInfo")) _PARAMS.Remove("signInfo");
            string strSHA256 = Generatesha256(_SECRET_KEY, signature);
            _PARAMS.Add("signInfo", strSHA256);

            #endregion Generate sha256 Code (signInfo)

            try
            {
                // For logging of rhe Params
                try
                {
                    string strDic = string.Join(";", _PARAMS.Select(x => x.Key + "=" + x.Value).ToArray());
                }
                catch { }

                respon TrustPayReponse = new respon();
                TrustPayReponse = HTTPPostReponse(_API_URL, _PARAMS);
                return TrustPayReponse;
            }
            catch (Exception e) { throw e; }
        }

        public static string Generatesha256(string key, string message)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(message));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
            //using (var hmac = HMAC.Create("HMACSHA256"))
            //{
            //    hmac.Key = Encoding.UTF8.GetBytes(key);
            //    byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            //    return BitConverter.ToString(signature).Replace("-", "").ToUpperInvariant();
            //}
        }

        private respon HTTPPostReponse(string requestURL, Dictionary<string, string> postDataParams)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var postValues = new NameValueCollection();
                    foreach (KeyValuePair<string, string> dic in postDataParams)
                        postValues[dic.Key] = dic.Value;

                    var response = client.UploadValues(requestURL, postValues);
                    string xml = Encoding.UTF8.GetString(response);
                    
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    XmlSerializer serializer = new XmlSerializer(typeof(respon));
                    using (TextReader reader = new StringReader(xml))
                    {
                        respon result = (respon)serializer.Deserialize(reader);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private bool validatePayload()
        {
            if (_PARAMS.Count() == 0)
            {
                throw new Exception("params can not be empty");
            }
            List<String> requiredParamList = new List<string>();

            requiredParamList.Add("merNo");
            requiredParamList.Add("gatewayNo");
            requiredParamList.Add("orderNo");
            requiredParamList.Add("orderCurrency");
            requiredParamList.Add("orderAmount");
            requiredParamList.Add("ccn");
            requiredParamList.Add("firstName");
            requiredParamList.Add("lastName");
            requiredParamList.Add("cardNo");
            requiredParamList.Add("cardExpireMonth");
            requiredParamList.Add("cardExpireYear");
            requiredParamList.Add("cardSecurityCode");
            requiredParamList.Add("issuingBank");
            requiredParamList.Add("email");
            requiredParamList.Add("ip");
            requiredParamList.Add("phone");
            requiredParamList.Add("country");
            requiredParamList.Add("city");
            requiredParamList.Add("address");
            requiredParamList.Add("zip");
            requiredParamList.Add("csid");

            foreach (var key in requiredParamList)
            {
                if (!_PARAMS.ContainsKey(key))
                {
                    throw new Exception(key + "param must have a value");
                }
            }

            return true;
        }

    }
}
