using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DAL;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TPLWeb.Tools
{
    public class PersianDateModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var modelType = bindingContext.ModelType;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrWhiteSpace(value))
            {
                if (IsNullable(modelType))
                {
                    bindingContext.Result = ModelBindingResult.Success(null);
                }
                return Task.CompletedTask;
            }

            value = ConvertToEngilshNumbers.ConvertToEngilshNumber(value.Trim());

            // Match: yyyy/MM/dd [HH:mm[:ss]]
            var m = Regex.Match(value, @"^(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})(?:\s+(\d{1,2}):(\d{1,2})(?::(\d{1,2}))?)?$");
            try
            {
                if (m.Success)
                {
                    var year = int.Parse(m.Groups[1].Value);
                    var month = int.Parse(m.Groups[2].Value);
                    var day = int.Parse(m.Groups[3].Value);
                    var hour = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
                    var minute = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;
                    var second = m.Groups[6].Success ? int.Parse(m.Groups[6].Value) : 0;

                    var pc = new PersianCalendar();
                    var dt = pc.ToDateTime(year, month, day, hour, minute, second, 0);

                    if (modelType == typeof(DateTime) || modelType == typeof(DateTime?))
                    {
                        bindingContext.Result = ModelBindingResult.Success(dt);
                        return Task.CompletedTask;
                    }

                    if (modelType == typeof(DateOnly) || modelType == typeof(DateOnly?))
                    {
                        var d = DateOnly.FromDateTime(dt);
                        bindingContext.Result = ModelBindingResult.Success(d);
                        return Task.CompletedTask;
                    }
                }

                // Fallback: try standard parsing (use current culture)
                if (modelType == typeof(DateTime) || modelType == typeof(DateTime?))
                {
                    if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDt) ||
                        DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDt))
                    {
                        bindingContext.Result = ModelBindingResult.Success(parsedDt);
                        return Task.CompletedTask;
                    }
                }

                if (modelType == typeof(DateOnly) || modelType == typeof(DateOnly?))
                {
                    if (DateOnly.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedDo) ||
                        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDo))
                    {
                        bindingContext.Result = ModelBindingResult.Success(parsedDo);
                        return Task.CompletedTask;
                    }
                }
            }
            catch
            {
                // ignored; will fall back to model state error below
            }

            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "فرمت تاریخ نامعتبر است. از الگوی yyyy/MM/dd استفاده کنید.");
            return Task.CompletedTask;
        }

        private static bool IsNullable(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }
    }

    public class PersianDateModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var type = context.Metadata.ModelType;
            if (type == typeof(DateTime) || type == typeof(DateTime?) || type == typeof(DateOnly) || type == typeof(DateOnly?))
            {
                return new PersianDateModelBinder();
            }

            return null;
        }
    }
}
