# 1. مرحلة البناء (Build Stage)
# نستخدم نسخة الـ SDK الكبيرة فقط لعمل Compile للكود
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات الـ csproj أولاً لعمل Restore (هذا يحسن الـ Caching)
COPY ["RecoMindAuthenticationAPI/RecoMindAuthenticationAPI.csproj", "RecoMindAuthenticationAPI/"]
COPY ["Authentication.Infrastructure/Authentication.Infrastructure.csproj", "Authentication.Infrastructure/"]
COPY ["Authentication.Core/Authentication.Core.csproj", "Authentication.Core/"]

# تنفيذ الـ Restore
RUN dotnet restore "RecoMindAuthenticationAPI/RecoMindAuthenticationAPI.csproj"

# نسخ باقي ملفات المشروع بالكامل
COPY . .

# الانتقال لمجلد المشروع الرئيسي وعمل Publish
WORKDIR "/src/RecoMindAuthenticationAPI"
RUN dotnet build "RecoMindAuthenticationAPI.csproj" -c Release -o /app/build
RUN dotnet publish "RecoMindAuthenticationAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
ENTRYPOINT ["dotnet", "RecoMindAuthenticationAPI.dll"]