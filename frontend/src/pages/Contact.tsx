import { useState } from 'react';
import { Link } from 'react-router-dom';
import { EnvelopeIcon, PhoneIcon, MapPinIcon, ArrowRightIcon } from '@heroicons/react/24/outline';
import toast from 'react-hot-toast';

export default function Contact() {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    subject: '',
    message: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    // Simulate form submission
    await new Promise(resolve => setTimeout(resolve, 1000));

    toast.success('Message sent! We\'ll get back to you soon.');
    setFormData({ name: '', email: '', subject: '', message: '' });
    setIsSubmitting(false);
  };

  const inputClasses = "block w-full bg-white/5 border border-white/10 rounded-lg text-white placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all sm:text-sm py-2 px-3";
  const labelClasses = "block text-sm font-medium text-gray-300 mb-1";

  return (
    <div className="min-h-screen bg-slate-900 text-white selection:bg-indigo-500 selection:text-white font-sans">

      {/* Navbar Overlay */}
      <header className="absolute top-0 left-0 right-0 z-50 pt-6 px-6 lg:px-8">
        <nav className="flex items-center justify-between max-w-7xl mx-auto rounded-2xl bg-white/5 backdrop-blur-lg border border-white/10 p-4">
          <div className="flex lg:flex-1">
            <Link to="/" className="text-2xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-white to-indigo-300">
              WorkFlow
            </Link>
          </div>
          <div className="hidden lg:flex lg:gap-x-8">
            <Link to="/" className="text-sm font-medium text-gray-300 hover:text-white transition-colors">Home</Link>
            <Link to="/about" className="text-sm font-medium text-gray-300 hover:text-white transition-colors">About Us</Link>
            <Link to="/contact" className="text-sm font-medium text-white">Contact</Link>
          </div>
          <div className="flex flex-1 justify-end gap-x-4 items-center">
            <Link to="/login" className="text-sm font-medium text-gray-300 hover:text-white transition-colors">
              Login
            </Link>
            <Link to="/register" className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-lg shadow-indigo-500/30 hover:bg-indigo-500 transition-all">
              Get Started
            </Link>
          </div>
        </nav>
      </header>

      {/* Content */}
      <main className="relative isolate pt-32 pb-16">
        {/* Background Gradients */}
        <div className="absolute top-0 left-0 w-full h-full overflow-hidden pointer-events-none -z-10">
          <div className="absolute top-[10%] right-[20%] w-[40%] h-[40%] rounded-full bg-purple-600/10 blur-[120px]" />
          <div className="absolute bottom-[10%] left-[10%] w-[30%] h-[50%] rounded-full bg-indigo-600/10 blur-[120px]" />
        </div>

        <div className="mx-auto max-w-7xl px-6 lg:px-8">
          <div className="mx-auto max-w-2xl text-center mb-16">
            <h1 className="text-4xl font-bold tracking-tight text-white sm:text-5xl bg-clip-text text-transparent bg-gradient-to-r from-white via-gray-200 to-gray-400">
              Contact Us
            </h1>
            <p className="mt-4 text-lg text-gray-400">
              Have questions? We'd love to hear from you. Send us a message and we'll respond as soon as possible.
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-16">
            {/* Contact Info */}
            <div>
              <h2 className="text-2xl font-bold text-white mb-6">Get in Touch</h2>

              <div className="space-y-6">
                <div className="flex items-start gap-4 p-4 rounded-xl hover:bg-white/5 transition-colors">
                  <div className="flex-shrink-0 w-10 h-10 rounded-full bg-indigo-600/20 flex items-center justify-center">
                    <EnvelopeIcon className="h-5 w-5 text-indigo-400" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-white">Email</h3>
                    <p className="text-gray-400">support@workflow.com</p>
                    <p className="text-gray-400">sales@workflow.com</p>
                  </div>
                </div>

                <div className="flex items-start gap-4 p-4 rounded-xl hover:bg-white/5 transition-colors">
                  <div className="flex-shrink-0 w-10 h-10 rounded-full bg-indigo-600/20 flex items-center justify-center">
                    <PhoneIcon className="h-5 w-5 text-indigo-400" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-white">Phone</h3>
                    <p className="text-gray-400">+1 (555) 123-4567</p>
                    <p className="text-sm text-gray-500">Mon-Fri 9am-6pm EST</p>
                  </div>
                </div>

                <div className="flex items-start gap-4 p-4 rounded-xl hover:bg-white/5 transition-colors">
                  <div className="flex-shrink-0 w-10 h-10 rounded-full bg-indigo-600/20 flex items-center justify-center">
                    <MapPinIcon className="h-5 w-5 text-indigo-400" />
                  </div>
                  <div>
                    <h3 className="font-semibold text-white">Office</h3>
                    <p className="text-gray-400">123 Business Street</p>
                    <p className="text-gray-400">Suite 100</p>
                    <p className="text-gray-400">New York, NY 10001</p>
                  </div>
                </div>
              </div>

              <div className="mt-10 p-6 bg-white/5 border border-white/10 rounded-2xl backdrop-blur-sm">
                <h3 className="font-semibold text-indigo-400 mb-2">Support Hours</h3>
                <div className="text-sm text-gray-400 space-y-1">
                  <p>Monday - Friday: 9:00 AM - 6:00 PM EST</p>
                  <p>Saturday: 10:00 AM - 4:00 PM EST</p>
                  <p>Sunday: Closed</p>
                </div>
              </div>
            </div>

            {/* Contact Form */}
            <div className="bg-white/5 border border-white/10 rounded-3xl p-8 backdrop-blur-md">
              <h2 className="text-2xl font-bold text-white mb-6">Send a Message</h2>

              <form onSubmit={handleSubmit} className="space-y-5">
                <div>
                  <label htmlFor="name" className={labelClasses}>
                    Your Name
                  </label>
                  <input
                    type="text"
                    id="name"
                    required
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className={inputClasses}
                    placeholder="John Doe"
                  />
                </div>

                <div>
                  <label htmlFor="email" className={labelClasses}>
                    Email Address
                  </label>
                  <input
                    type="email"
                    id="email"
                    required
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    className={inputClasses}
                    placeholder="john@example.com"
                  />
                </div>

                <div>
                  <label htmlFor="subject" className={labelClasses}>
                    Subject
                  </label>
                  <input
                    type="text"
                    id="subject"
                    required
                    value={formData.subject}
                    onChange={(e) => setFormData({ ...formData, subject: e.target.value })}
                    className={inputClasses}
                    placeholder="How can we help?"
                  />
                </div>

                <div>
                  <label htmlFor="message" className={labelClasses}>
                    Message
                  </label>
                  <textarea
                    id="message"
                    rows={5}
                    required
                    value={formData.message}
                    onChange={(e) => setFormData({ ...formData, message: e.target.value })}
                    className={inputClasses}
                    placeholder="Tell us about your project..."
                  />
                </div>

                <div className="pt-2">
                  <button
                    type="submit"
                    disabled={isSubmitting}
                    className="w-full rounded-lg bg-indigo-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-indigo-500/30 hover:bg-indigo-500 hover:shadow-indigo-500/40 hover:-translate-y-0.5 transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                  >
                    {isSubmitting ? 'Sending...' : 'Send Message'}
                    {!isSubmitting && <ArrowRightIcon className="h-4 w-4" />}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="bg-black/40 border-t border-white/10 py-12 mt-16">
        <div className="mx-auto max-w-7xl px-6 lg:px-8">
          <div className="flex flex-col md:flex-row justify-between items-center">
            <div className="flex items-center gap-2 mb-4 md:mb-0">
              <span className="text-xl font-bold text-white">WorkFlow</span>
            </div>
            <div className="flex gap-x-8 text-sm text-gray-400">
              <Link to="/about" className="hover:text-white transition-colors">About</Link>
              <Link to="/contact" className="hover:text-white transition-colors">Contact</Link>
              <Link to="/login" className="hover:text-white transition-colors">Login</Link>
            </div>
          </div>
          <p className="mt-8 text-center text-xs text-gray-500">
            © {new Date().getFullYear()} WorkFlow. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  );
}
