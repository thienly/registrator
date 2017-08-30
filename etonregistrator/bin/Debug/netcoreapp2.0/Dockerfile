FROM microsoft/aspnetcore:2.0
ARG source
WORKDIR /app
EXPOSE 80
COPY . ./
ENTRYPOINT ["dotnet", "etonregistrator.dll"]
HEALTHCHECK --interval=5s --timeout=3s \
  CMD powershell Invoke-WebRequest http://localhost/api/consul || exit 1
