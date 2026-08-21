import { type FormEvent, useState } from "react";
import { useNavigate } from "react-router-dom";

import { useAuth } from "../features/auth/auth-context";
import { GoogleSignInButton } from "../features/auth/components/google-sign-in-button";
import { Button } from "../shared/ui/button";
import { TextField } from "../shared/ui/field";
import { MoochIcon } from "../shared/ui/mooch-icon";

export function LandingPage() {
  const navigate = useNavigate();
  const { login, register, socialLogin, socialRegister } = useAuth();
  const [mode, setMode] = useState<"login" | "create">("login");
  const [displayName, setDisplayName] = useState("Demo Walker");
  const [email, setEmail] = useState("john@example.com");
  const [password, setPassword] = useState("Password123!");
  const [formError, setFormError] = useState("");

  async function handleEmailLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError("");

    if (!email.trim() || !password.trim()) {
      setFormError("Email and password are required.");
      return;
    }

    try {
      await login(email, password);
      navigate("/app");
    } catch (error) {
      setFormError(error instanceof Error ? error.message : "Unable to log in.");
    }
  }

  async function handleCreateAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setFormError("");

    if (!displayName.trim() || !email.trim() || !password.trim()) {
      setFormError("Full name, email, and password are required.");
      return;
    }

    try {
      await register(email, displayName, password);
      navigate("/app/onboarding");
    } catch (error) {
      setFormError(error instanceof Error ? error.message : "Unable to create an account.");
    }
  }

  async function handleGoogleCredential(credential: string) {
    setFormError("");

    try {
      if (mode === "login") {
        await socialLogin("google", credential);
      } else {
        await socialRegister("google", credential);
      }

      navigate(mode === "login" ? "/app" : "/app/onboarding");
    } catch (error) {
      setFormError(
        error instanceof Error
          ? error.message
          : mode === "login"
            ? "Unable to sign in with Google."
            : "Unable to create your account with Google."
      );
    }
  }

  return (
    <div className="landing-page">
      <header className="landing-page__header">
        <div className="brand-row">
          <div className="brand-mark">
            <MoochIcon name="paw" />
          </div>
          <div>
            <div className="brand-name">Mooch</div>
            <div className="brand-tagline">Social fitness for dogs, their humans, and the pack around them</div>
          </div>
        </div>

        <div className="landing-page__actions">
          <Button onClick={() => setMode("login")} variant="secondary">
            Log In
          </Button>
          <Button onClick={() => setMode("create")}>
            Join for Free
          </Button>
        </div>
      </header>

      <main className="landing-page__hero">
        <div className="landing-page__content">
          <p className="eyebrow">Dogs first</p>
          <h1>Track the walks that matter, and the furry friends who came along.</h1>
          <p className="landing-page__copy">
            Mooch helps owners, walkers, and dog-loving friends log adventures, keep tabs on each doggo,
            and stay close to the local pack.
          </p>

          <div className="landing-page__cta-row">
            <Button onClick={() => setMode("create")}>
              Create account
            </Button>
            <Button onClick={() => setMode("login")} variant="secondary">
              Sign in
            </Button>
          </div>
        </div>

        <div className="landing-page__card">
          <h2>{mode === "login" ? "Sign in with" : "Create account with"}</h2>

          <GoogleSignInButton
            mode={mode === "login" ? "signin" : "signup"}
            onCredential={handleGoogleCredential}
            onError={setFormError}
          />

          <button className="social-button" type="button" disabled>
            {mode === "login" ? "Sign in with Apple" : "Create account with Apple"}
          </button>

          <p className="brand-tagline">
            {mode === "login"
              ? "Sign in is for existing Mooch profiles. If your profile does not exist yet, create your account first."
              : "Create your Mooch account with Google, Apple, or email. After that we will guide you into your profile and first dog."}
          </p>

          <div className="divider">or use email</div>

          {mode === "login" ? (
            <form onSubmit={handleEmailLogin}>
              <p className="field-helper">Use the email already attached to your Mooch profile.</p>
              <TextField
                id="landing-email"
                label="Email"
                onChange={(event) => setEmail(event.target.value)}
                type="email"
                value={email}
              />
              <TextField
                id="landing-password"
                label="Password"
                onChange={(event) => setPassword(event.target.value)}
                type="password"
                value={password}
              />

              <label className="checkbox-row">
                <input type="checkbox" defaultChecked />
                <span>Remember me</span>
              </label>

              {formError && <p className="form-error">{formError}</p>}

              <Button fullWidth type="submit">
                Sign in with email
              </Button>
            </form>
          ) : (
            <form onSubmit={handleCreateAccount}>
              <p className="field-helper">Create your human profile first. Dog profile setup comes right after.</p>
              <TextField
                id="create-name"
                label="Full Name"
                onChange={(event) => setDisplayName(event.target.value)}
                placeholder="Sasha Walker"
                type="text"
                value={displayName}
              />
              <TextField
                id="create-email"
                label="Email"
                onChange={(event) => setEmail(event.target.value)}
                placeholder="you@example.com"
                type="email"
                value={email}
              />
              <TextField
                id="create-password"
                label="Password"
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Create a password"
                type="password"
                value={password}
              />

              {formError && <p className="form-error">{formError}</p>}

              <Button fullWidth type="submit">
                Create account with email
              </Button>
            </form>
          )}

          <Button fullWidth onClick={() => setMode(mode === "login" ? "create" : "login")} variant="text">
            {mode === "login" ? "Need a Mooch account? Create one" : "Already have an account? Sign in"}
          </Button>
        </div>
      </main>
    </div>
  );
}
