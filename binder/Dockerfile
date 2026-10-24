# The image mybinder.org builds for the README's "Try" button: Jupyter with the .NET Interactive
# kernels, and the sample notebooks from Sources/Samples. The notebooks install AngouriMath.Interactive
# from NuGet themselves. https://github.com/asc-community/AngouriMath/issues/1689
FROM quay.io/jupyter/base-notebook:2026-10-05

USER root
RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends curl libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# The .NET 10 SDK, which the dotnet-interactive tool targets.
ENV DOTNET_ROOT=/usr/share/dotnet \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    NUGET_XMLDOC_MODE=skip
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && bash /tmp/dotnet-install.sh --channel 10.0 --install-dir ${DOTNET_ROOT} \
    && ln -s ${DOTNET_ROOT}/dotnet /usr/bin/dotnet \
    && rm /tmp/dotnet-install.sh

COPY Sources/Samples/Interactive.Sample.ipynb Sources/Samples/CSharp.Interactive.Sample.ipynb ${HOME}/
RUN chown -R ${NB_UID}:${NB_GID} ${HOME}

USER ${NB_USER}
ENV PATH="${PATH}:${HOME}/.dotnet/tools"
RUN dotnet tool install -g Microsoft.dotnet-interactive \
    && dotnet interactive jupyter install
WORKDIR ${HOME}
