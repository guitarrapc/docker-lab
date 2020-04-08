FROM composer:latest

# install node
RUN apk add bash curl nodejs
# install yarn
RUN touch ~/.bashrc \
  && curl -o- -L https://yarnpkg.com/install.sh | bash \
  && ln -s "$HOME/.yarn/bin/yarn" /usr/local/bin/yarn

WORKDIR /tmp

COPY ./app/composer.json ./composer.json
COPY ./app/composer.lock ./composer.lock
RUN composer install

COPY ./app/package.json ./package.json
COPY ./app/yarn.lock ./yarn.lock
RUN yarn install

WORKDIR /app
COPY ./app/entrypoint.sh .

CMD [ "./entrypoint.sh" ]