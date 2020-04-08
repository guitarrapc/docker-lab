FROM composer:latest

# install node
RUN apk add bash curl nodejs
# install yarn
RUN touch ~/.bashrc \
  && curl -o- -L https://yarnpkg.com/install.sh | bash \
  && ln -s "$HOME/.yarn/bin/yarn" /usr/local/bin/yarn
