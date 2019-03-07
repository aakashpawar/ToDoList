FROM microsoft/dotnet:2.2-aspnetcore-runtime
ENTRYPOINT ["dotnet", "ToDo.Web.dll"]
ARG source=.
WORKDIR /app
COPY $source .