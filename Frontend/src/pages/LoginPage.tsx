import { useState } from "react";
import type { FormEvent } from "react";

const PASSWORD_RULE = /^(?=.*\d)(?=.*[!@#$%^&*]).{8,20}$/;
const PASSWORD_HINT =
  "8-20 characters, at least one digit and one of !@#$%^&*";

interface Props {
  onLogin: (username: string, password: string) => Promise<void>;
  onRegister: (username: string, password: string) => Promise<void>;
}

type Mode = "login" | "register";

export default function LoginPage({ onLogin, onRegister }: Props) {
  const [mode, setMode] = useState<Mode>("login");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    if (mode === "register" && !PASSWORD_RULE.test(password)) {
      setError(PASSWORD_HINT);
      return;
    }

    setSubmitting(true);
    try {
      if (mode === "register") {
        await onRegister(username, password);
      }
      await onLogin(username, password);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-cream flex items-center justify-center px-5">
      <div
        className="w-full max-w-sm rounded-2xl p-6"
        style={{
          background: "#FDFBF6",
          border: "1px solid #DDD4BF",
          boxShadow: "0 2px 10px rgba(44,26,14,0.06)",
        }}
      >
        <h1 className="font-serif text-2xl font-semibold text-bark text-center mb-1">
          Book Tracker
        </h1>
        <p className="text-muted text-sm text-center mb-6">
          {mode === "login" ? "Sign in to your library" : "Create an account"}
        </p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <input
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="Username"
            autoComplete="username"
            required
            className="w-full rounded-xl px-4 py-3 text-sm outline-none"
            style={{ background: "#EAE2D0", color: "#5C3D28" }}
          />
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Password"
            autoComplete={
              mode === "login" ? "current-password" : "new-password"
            }
            required
            className="w-full rounded-xl px-4 py-3 text-sm outline-none"
            style={{ background: "#EAE2D0", color: "#5C3D28" }}
          />

          {mode === "register" && (
            <p className="text-[11px] text-muted-light">{PASSWORD_HINT}</p>
          )}

          {error && (
            <p className="text-xs" style={{ color: "#A85830" }}>
              {error}
            </p>
          )}

          <button
            type="submit"
            disabled={submitting}
            className="w-full py-3 rounded-xl text-sm font-semibold text-white transition-all active:scale-[0.99] disabled:opacity-60"
            style={{ background: "linear-gradient(135deg, #b46bdc, #934dee)" }}
          >
            {submitting
              ? "Please wait…"
              : mode === "login"
                ? "Sign In"
                : "Create Account"}
          </button>
        </form>

        <button
          onClick={() => {
            setMode((m) => (m === "login" ? "register" : "login"));
            setError(null);
          }}
          className="w-full text-center text-xs text-muted hover:text-bark transition-colors mt-4"
        >
          {mode === "login"
            ? "Need an account? Register"
            : "Already have an account? Sign in"}
        </button>
      </div>
    </div>
  );
}
