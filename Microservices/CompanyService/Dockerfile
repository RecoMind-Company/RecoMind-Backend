# 1. مرحلة البناء (Build Stage)
# نستخدم نسخة الـ SDK الكبيرة فقط لعمل Compile للكود
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات الـ csproj أولاً لعمل Restore (هذا يحسن الـ Caching)
COPY ["webApi/webApi.csproj", "webApi/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# تنفيذ الـ Restore
RUN dotnet restore "webApi/webApi.csproj"

# نسخ باقي ملفات المشروع بالكامل
COPY . .

# الانتقال لمجلد المشروع الرئيسي وعمل Publish
WORKDIR "/src/webApi"
RUN dotnet build "webApi.csproj" -c Release -o /app/build
RUN dotnet publish "webApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. مرحلة التشغيل (Runtime Stage)
# نستخدم نسخة الـ ASP.NET الرفيعة (Runtime) لتصغير حجم الصورة للأمان والسرعة
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# فتح المنفذ 80 و 443 (اختياري حسب إعداداتك)
EXPOSE 80
EXPOSE 443

# نسخ الملفات النهائية فقط من مرحلة البناء
COPY --from=build /app/publish .

# تحديد نقطة الانطلاق (اسم الـ DLL الخاص بمشروعك)
ENTRYPOINT ["dotnet", "webApi.dll"]