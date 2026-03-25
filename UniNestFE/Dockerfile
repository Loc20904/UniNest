# GIAI ĐOẠN 1: Build dự án bằng .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

# Copy mã nguồn và Publish
COPY . .
RUN dotnet publish UniNestFE.csproj -c Release -o /app/publish

# GIAI ĐOẠN 2: Chạy ứng dụng bằng Nginx (Web Server siêu nhẹ)
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Xóa trang web mặc định của Nginx
RUN rm -rf ./*

# BÍ QUYẾT Ở ĐÂY: Copy toàn bộ nội dung trong thư mục 'wwwroot' của Blazor sang Nginx
COPY --from=build /app/publish/wwwroot .

# Nginx mặc định lắng nghe ở cổng 80
EXPOSE 80