import {
  createCookieSessionStorage,
  Session,
  SessionData,
} from "@remix-run/node";

const sessionStorage = createCookieSessionStorage({
  cookie: {
    name: "__session",
    httpOnly: true,
    path: "/",
    sameSite: "lax",
    secrets: ["s3cr3t"],
    secure: process.env.NODE_ENV === "production",
    maxAge: 60 * 60 * 24 * 14, // 14 days
  },
});

export function getSession(request: Request) {
  return sessionStorage.getSession(request.headers.get("Cookie"));
}

export function commitSession(session: Session<SessionData, SessionData>) {
  return sessionStorage.commitSession(session);
}

export function destroySession(session: Session<SessionData, SessionData>) {
  return sessionStorage.destroySession(session);
}
