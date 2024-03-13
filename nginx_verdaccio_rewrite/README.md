# tl;dr;

Handling Verdaccio hosted on https & other domain, http://localhost:8080 -> https://verdaccio.example.com

## Description

There are some conciderations on proxying Verdaccio from other domain:

1. npm client can handle GET redirect but cannot for client-side POST/PUT redirect. It will cause 405 error on `npm adduser` and `npm publish`.
2. Verdaccio Web Page has CSP `connect-src 'self'`, there are rewrite host problem when access from other domain.

To solve the problem, we need to:

1. Web access: Redirect to upstream.
2. NPM access: Proxy to upstream and rewrite host.

This nginx configuration will handle these problems.
