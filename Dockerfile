# Set the base image as the .NET 6.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

ARG rid=linux-x64

# Copy everything and publish the release (publish implicitly restores and builds)
WORKDIR /app
COPY . ./
RUN dotnet publish ./SourcePawnManager/SourcePawnManager.csproj -c Release -o out -r $rid --no-self-contained -p:PublishReadyToRun=true
RUN cp COPYING out/COPYING
RUN cp licensing.txt out/licensing.txt


# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/runtime:7.0 as spm
COPY --from=build-env /app/out /usr/share/spm
RUN ln -s /usr/share/spm/spm /usr/bin/spm
ENTRYPOINT [ "spm" ]

# Label the container
LABEL maintainer="icebear <icebear@icebear.rocks>"
LABEL repository="https://github.com/icebear/spm"
LABEL homepage="https://github.com/icebear/spm"


# Relayer the .NET SDK, anew with the build output
FROM spm as spm-gh-action
COPY action.sh /action.sh
RUN chmod +x action.sh
ENTRYPOINT [ "/action.sh" ]

# Label as GitHub action
LABEL com.github.actions.name="SourcePawnManager"
# Limit to 160 characters
LABEL com.github.actions.description="restore includes defined in spm.json"
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="aperture"
LABEL com.github.actions.color="blue"