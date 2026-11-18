
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BloodDonorApp.Services
{
    public class PinCodeValidationService
    {
        private readonly HttpClient _httpClient;

        public PinCodeValidationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PinCodeResult> ValidateAsync(
            string pinCode)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"https://api.postalpincode.in/pincode/{pinCode}");

                if (!response.IsSuccessStatusCode)
                {
                    return new PinCodeResult
                    {
                        Error = "We couldn't verify the PIN code right now. Please try again."
                    };
                }

                var data = await response.Content
                    .ReadFromJsonAsync<List<PostalResponse>>();

                if (data == null || data.Count == 0)
                {
                    return new PinCodeResult
                    {
                        Error = "We couldn't verify the PIN code right now. Please try again."
                    };
                }

                var result = data[0];

                if (result.Status == "Error")
                {
                    return new PinCodeResult
                    {
                        Error = "This PIN code was not found. Please check and try again."
                    };
                }

                if (result.Status != "Success" ||
                    result.PostOffice == null ||
                    result.PostOffice.Count == 0)
                {
                    return new PinCodeResult
                    {
                        Error = "We couldn't verify the PIN code right now. Please try again."
                    };
                }

                var states = result.PostOffice
                    .Where(p => !string.IsNullOrWhiteSpace(p.State))
                    .Select(p => p.State!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (states.Count == 0)
                {
                    return new PinCodeResult
                    {
                        Error = "We couldn't verify the PIN code right now. Please try again."
                    };
                }

                return new PinCodeResult
                {
                    IsValid = true,
                    States = states
                };
            }
            catch (HttpRequestException)
            {
                return new PinCodeResult
                {
                    Error = "PIN code verification is temporarily unavailable. Please try again."
                };
            }
            catch (TaskCanceledException)
            {
                return new PinCodeResult
                {
                    Error = "PIN code verification timed out. Please try again."
                };
            }
            catch (System.Text.Json.JsonException)
            {
                return new PinCodeResult
                {
                    Error = "We couldn't read the PIN code verification response."
                };
            }
        }
    }

    public class PinCodeResult
    {
        public bool IsValid { get; set; }

        public List<string> States { get; set; } = new();

        public string? Error { get; set; }
    }

    public class PostalResponse
    {
        public string? Status { get; set; }

        [JsonPropertyName("PostOffice")]
        public List<PostalOffice>? PostOffice { get; set; }
    }

    public class PostalOffice
    {
        public string? State { get; set; }
    }
}
